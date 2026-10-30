import { Browser, isUndefined, createElement, closest , isNullOrUndefined} from '@syncfusion/ej2-base';
import { parentsUntil } from './util';
import { SfGrid } from './sf-grid-fn';
import { Column, ISelectedCell, IPasteCell } from './interfaces';

/**
 * The `Clipboard` module is used to handle clipboard copy action.
 */
export class Clipboard {
    //Internal variables
    private activeElement: Element;
    protected clipBoardTextArea: HTMLInputElement;
    private copyContent: string = '';
    private isSelect: boolean = false;
    //Module declarations
    private parent: SfGrid;

    constructor(parent?: SfGrid) {
        this.parent = parent;
        this.clipBoardTextArea = createElement('textarea', {
            className: 'e-clipboard',
            styles: 'opacity: 0',
            attrs: { tabindex: '-1', 'aria-label': 'clipboard' }
        }) as HTMLInputElement;
        this.parent.element.appendChild(this.clipBoardTextArea);
    }

    public pasteHandler(): void {
        const grid: SfGrid = this.parent;
        const active = document.activeElement as HTMLElement | null;
        const target = (active && closest(active, '.e-rowcell')) as HTMLElement
            ?? (this.parent.element.querySelector('.e-rowcell.e-cellselectionbackground') as HTMLElement)
            ?? (this.parent.element.querySelector('.e-rowcell[tabindex="0"]') as HTMLElement);
        if (!target || !this.isPasteAllowed() ||
            (target.classList.contains('e-editedbatchcell') && grid.options.cellSelectionMode === 'Box')) {
            return;
        }
        this.activeElement = document.activeElement;
        this.clipBoardTextArea.value = '';
        const x: number = window.scrollX;
        const y: number = window.scrollY;
        this.clipBoardTextArea.focus();
        setTimeout(
            () => {
                (this.activeElement as HTMLInputElement).focus();
                window.scrollTo(x, y);
                const name: string = 'Paste';
                grid.dotNetRef.invokeMethodAsync('InvokeCopyPasteAction', {
                    clipboardText: this.clipBoardTextArea.value
                }, name);
            }, 10);
    }

    /**
     * When a single cell value is copied and pasted, the value is applied
     * to all selected target cells in Batch Edit mode. The method reuses
     * that validation, BeforeCellPaste events, UpdateCell processing,
     * and dirty tracking continue to work without modification.
     *
     * @param selection Collection of selected cell ranges.
     * @param value Clipboard value to be applied to each selected cell.
     */

    private getSelectionPasteCells(selection: ISelectedCell[], value: string): IPasteCell[] {
        const pasteCells: IPasteCell[] = [];

        if (!this.isPasteAllowed() || !selection || selection.length === 0) {
            return pasteCells;
        }

        for (const cellGroup of selection) {
            const rowIndex: number = cellGroup.rowIndex;

            for (const cellIndex of cellGroup.cellIndexes) {
                const column: Column =
                    this.parent.getColumnByIndex(cellIndex);

                if (!this.isValidPasteTarget(column)) {
                    continue;
                }

                pasteCells.push({ rowIndex: rowIndex, columnIndex: cellIndex, columnField: column.field, value: value });
            }
        }

        return pasteCells;
    }

    private isValidPasteTarget(column: Column): boolean {
        return !!column &&
            column.allowEditing &&
            !column.isPrimaryKey &&
            !column.template &&
            !column.commands;
    }

    private isValidCopyTarget(column: Column): boolean {
        return !!column &&
            !column.template &&
            !column.commands;
    }

    private isEditModeAllowed(): boolean {
        return this.parent.options.editMode === 'Batch' || this.parent.options.editMode === 'Cell';
    }

    private isPasteAllowed(): boolean {
        return this.parent.options.allowEditing &&
            this.isEditModeAllowed() &&
            this.parent.options.selectionMode === 'Cell' &&
            this.parent.options.cellSelectionMode !== 'Flow';
    }

    private getMatrixPasteCells(data: string, rowIndex: number, colIndex: number): IPasteCell[] {
        const grid: SfGrid = this.parent;
        const pasteCells: IPasteCell[] = [];
        let currentRowIndex: number = rowIndex;
        if (!this.isPasteAllowed()) {
            return pasteCells;
        }
        /*
         * Preserve the existing single-cell multiline handling.
         */
        const trimmedData: string = data.trim();

        const isSingleCellMultiline: boolean = trimmedData.startsWith('"') && trimmedData.endsWith('"');

        if (isSingleCellMultiline) {
            const value: string = trimmedData.slice(1, -1).replace(/\\n/g, '\n');
            const column: Column = grid.getColumnByIndex(colIndex);

            if (!isNullOrUndefined(column) && this.isValidPasteTarget(column)) {
                pasteCells.push({ rowIndex: rowIndex, columnIndex: colIndex, columnField: column.field, value: value });
            }

            return pasteCells;
        }

        const rows: string[] = data.split('\n');
        const dataRows: HTMLElement[] = grid.getDataRows() as HTMLElement[];
        let movableRows: HTMLElement[] = [];
        const isFrozen: number = this.parent.options.frozenColumns;

        if (isFrozen) {
            movableRows = grid.getMovableDataRows() as HTMLElement[];
        }

        for (let row: number = 0; row < rows.length; row++) {
            // eslint-disable-next-line security/detect-object-injection
            const rowValue: string = rows[row];

            if (
                row === rows.length - 1 &&
                rowValue === ''
            ) {
                break;
            }

            if (isUndefined(grid.getRowByIndex(currentRowIndex))) {
                break;
            }

            const columns: string[] = rowValue.split('\t');
            let currentColumnIndex: number = colIndex;

            for (
                let columnIndex: number = 0;
                columnIndex < columns.length;
                columnIndex++
            ) {
                let isAvailable: Element | boolean = grid.getCellFromIndex(currentRowIndex, currentColumnIndex);

                if (isFrozen) {
                    // eslint-disable-next-line security/detect-object-injection
                    const frozenRow: HTMLElement = dataRows[currentRowIndex];
                    // eslint-disable-next-line security/detect-object-injection
                    const movableRow: HTMLElement = movableRows[currentRowIndex];

                    if (!frozenRow || !movableRow) {
                        break;
                    }

                    isAvailable = !frozenRow.querySelector('[aria-colindex="' + (currentColumnIndex + 1) + '"]')
                        ? movableRow.querySelector('[aria-colindex="' + (currentColumnIndex + 1) + '"]')
                        : true;
                }

                if (!isAvailable) {
                    break;
                }

                const column: Column =
                    grid.getColumnByIndex(currentColumnIndex);

                if (this.isValidPasteTarget(column)) {
                    // eslint-disable-next-line security/detect-object-injection
                    pasteCells.push({ rowIndex: currentRowIndex, columnIndex: currentColumnIndex, columnField: column.field, value: columns[columnIndex] });
                }

                currentColumnIndex++;
            }

            currentRowIndex++;
        }

        return pasteCells;
    }

    private async invokeBatchPasteAction(pasteCells: IPasteCell[]): Promise<boolean> {
        if (!pasteCells || pasteCells.length === 0) {
            return false;
        }

        try {
            const processedCells: IPasteCell[] = await (this.parent.dotNetRef.invokeMethodAsync('InvokeBatchPasteAction', pasteCells) as unknown as Promise<IPasteCell[]>);
            if (!processedCells || processedCells.length === 0) {
                return false;
            }
            return true;
        } catch (error) {
            // Keep during development to expose interop/deserialization errors.
            console.error('InvokeBatchPasteAction failed.', error);
            return false;
        }
    }

    public async pasteBatch(selectedCells: ISelectedCell[], value: string): Promise<boolean> {
        if (!selectedCells || selectedCells.length === 0 || !value
        ) {
            return Promise.resolve(false);
        }
        const isSingleCellSource: boolean = !value.includes('\n') && !value.includes('\t');
        let pasteCells: IPasteCell[];
        if (isSingleCellSource) {
            if(this.parent.options.enableUndoRedo && this.parent.options.editMode === 'Batch') {
                await this.parent.dotNetRef.invokeMethodAsync('BeginPasteBatch');
            }
            pasteCells = this.getSelectionPasteCells(selectedCells, value);
        } else {
            if(this.parent.options.enableUndoRedo && this.parent.options.editMode === 'Batch') {
                await this.parent.dotNetRef.invokeMethodAsync('BeginPasteBatch');
            }
            pasteCells = this.getMatrixPasteCells(value, selectedCells[0].rowIndex, selectedCells[0].cellIndexes[0]);
        }

        const result = await this.invokeBatchPasteAction(pasteCells);
        if(this.parent.options.enableUndoRedo && this.parent.options.editMode === 'Batch') {
            await this.parent.dotNetRef.invokeMethodAsync('CommitPasteBatch');
        }
        return result;
    }

    protected setCopyData(withHeader?: boolean): void {
        if (window.getSelection().toString() === '') {
            this.clipBoardTextArea.value = this.copyContent = '';
            const rows: Element[] = this.parent.getRows();
            if (this.parent.options.selectionMode !== 'Cell') {
                //let selectedIndexes: Object[] = this.parent.getSelectedRowIndexes().sort((a: number, b: number) => { return a - b; });
                const selectedIndexes: number[] = this.parent.getSelectedRowIndexes(this.parent.options.enableVirtualization);
                if (withHeader) {
                    const headerTextArray: string[] = [];
                    for (let i: number = 0; i < this.parent.getVisibleColumns().length; i++) {
                        headerTextArray[parseInt(i.toString(), 10)] =
                        this.parent.getVisibleColumns()[parseInt(i.toString(), 10)].headerText;
                    }
                    this.getCopyData(headerTextArray, false, '\t', withHeader);
                    this.copyContent += '\n';
                }
                for (let i: number = 0; i < selectedIndexes.length; i++) {
                    if (i > 0) {
                        this.copyContent += '\n';
                    }
                    if (parseInt(rows[0].getAttribute('aria-rowindex'), 10) !== 1 && this.parent.options.selectionMode !== 'Both') {
                        selectedIndexes[parseInt(i.toString(), 10)] = selectedIndexes[parseInt(i.toString(), 10)] - parseInt(rows[0].getAttribute('aria-rowindex'), 10) + 1;
                    }
                    const cells: HTMLElement[] = [].slice.call(rows[selectedIndexes[parseInt(i.toString(), 10)] as number].querySelectorAll('.e-rowcell:not(.e-hide)'));
                    this.getCopyData(cells, false, '\t', withHeader);
                }
            } else {
                const obj: { status: boolean, rowIndexes?: number[], colIndexes?: number[] } = this.checkBoxSelection();
                if (obj.status) {
                    if (withHeader) {
                        const headers: HTMLElement[] = [];
                        for (let i: number = 0; i < obj.colIndexes.length; i++) {
                            headers.push(this.parent.getColumnHeaderByIndex(obj.colIndexes[parseInt(i.toString(), 10)]) as HTMLElement);
                        }
                        this.getCopyData(headers, false, '\t', withHeader);
                        this.copyContent += '\n';
                    }
                    for (let i: number = 0; i < obj.rowIndexes.length; i++) {
                        if (i > 0) {
                            this.copyContent += '\n';
                        }
                        const cells: HTMLElement[] = [].slice.call(rows[obj.rowIndexes[parseInt(i.toString(), 10)] as number].
                            querySelectorAll('.e-cellselectionbackground'));
                        this.getCopyData(cells, false, '\t', withHeader);
                    }
                } else {
                    this.getCopyData(
                        [].slice.call(this.parent.element.querySelectorAll('.e-cellselectionbackground')),
                        true, '\n', withHeader);
                }
            }
            const name: string = 'Copy';
            if (this.parent.options.isClipboardEventBinded) {
                this.parent.dotNetRef.invokeMethodAsync('InvokeCopyPasteAction', {
                    clipboardText: this.copyContent
                }, name);
            } else {
                this.clipBoardData(false, this.copyContent);
            }
        } else if (parentsUntil(document.activeElement, 'e-grid')) {
            const clipboardFocus: Element | null = document.activeElement;
            const selectedText: string = window.getSelection().toString();
            if (selectedText) {
                if (navigator.clipboard) {
                    navigator.clipboard.writeText(selectedText).then(() => {
                        this.clipBoardTextArea.blur();
                        (clipboardFocus as HTMLElement).focus();
                    });
                }
            }
        }
    }

    private clipBoardData(cancel: boolean, data: string): void {
        const clipboardFocus: Element = document.activeElement;
        this.copyContent = data !== '' ? data : this.copyContent;
        if (!cancel) {
            this.clipBoardTextArea.value = this.copyContent;
            if (!Browser.userAgent.match(/ipad|ipod|iphone/i)) {
                this.clipBoardTextArea.select();
            } else {
                this.clipBoardTextArea.setSelectionRange(0, this.clipBoardTextArea.value.length);
            }
            this.isSelect = true;
            if (navigator.clipboard) {
                navigator.clipboard.writeText(this.copyContent).then(() => {
                    this.clipBoardTextArea.blur();
                    (clipboardFocus as HTMLElement).focus({ preventScroll: true });
                });
            }
            if (this.isSelect) {
                window.getSelection().removeAllRanges();
                this.isSelect = false;
            }
        }
    }

    private getCopyData(cells: HTMLElement[] | string[], isCell: boolean, splitKey: string, withHeader?: boolean): void {
        const isElement: boolean = typeof cells[0] !== 'string';
        for (let j: number = 0; j < cells.length; j++) {
            if (isElement) {
                // eslint-disable-next-line security/detect-object-injection
                const colIndex: number = parseInt((cells[j] as HTMLElement).getAttribute('aria-colindex'), 10) - 1;
                const column: Column = this.parent.getColumnByIndex(colIndex);
                if (!this.isValidCopyTarget(column)) {
                    continue;
                }
            }
            if (withHeader && isCell) {
                this.copyContent += (this.parent.getColumns() as Column[])[parseInt((cells[parseInt(j.toString(), 10)] as HTMLElement).getAttribute('aria-colindex'), 10) - 1].headerText + '\n';
            }
            if (isElement) {
                if (!(cells[parseInt(j.toString(), 10)] as HTMLElement).classList.contains('e-hide')) {
                    if ((!(cells[parseInt(j.toString(), 10)] as HTMLElement).classList.contains('e-gridchkbox')) &&
                        Object.keys((cells[parseInt(j.toString(), 10)] as HTMLElement).querySelectorAll('.e-check')).length) {
                        this.copyContent += true;
                    } else if ((!(cells[parseInt(j.toString(), 10)] as HTMLElement).classList.contains('e-gridchkbox')) &&
                        Object.keys((cells[parseInt(j.toString(), 10)] as HTMLElement).querySelectorAll('.e-uncheck')).length) {
                        this.copyContent += false;
                    } else {
                        this.copyContent += (cells[parseInt(j.toString(), 10)] as HTMLElement).innerText;
                    }
                }
            } else {
                this.copyContent += cells[parseInt(j.toString(), 10)];
            }
            if (j < cells.length - 1) {
                this.copyContent += splitKey;
            }
        }
    }

    public copy(withHeader?: boolean): void {
        if (navigator.clipboard) {
            this.setCopyData(withHeader);
        }
    }

    private getSelectedRowCellIndexes(): ISelectedCell[] {
        const gridObj: SfGrid = this.parent;
        const rowCellIndxes: ISelectedCell[] = [];
        const rows: Element[] = gridObj.getRows();
        let mrows: Element[] = [];
        if (gridObj.options.frozenColumns) {
            const movableDataRows = gridObj.getMovableDataRows();
            if (!isNullOrUndefined(movableDataRows)) {
                mrows = movableDataRows;
            }
        }
        for (let i: number = 0; i < rows.length; i++) {
            let tempCells: NodeList = rows[parseInt(i.toString(), 10)].querySelectorAll('.e-cellselectionbackground');
            if (gridObj.options.frozenColumns && !tempCells.length && mrows.length > i) {
                tempCells = mrows[parseInt(i.toString(), 10)].querySelectorAll('.e-cellselectionbackground');
            }
            if (tempCells.length) {
                const cellIndexes: number[] = [];
                tempCells.forEach((element: HTMLElement) => {
                    cellIndexes.push(parseInt(element.getAttribute('aria-colindex'), 10) - 1);
                });
                rowCellIndxes.push({ rowIndex: i, cellIndexes: cellIndexes });
            }
        }
        return rowCellIndxes;
    }

    private checkBoxSelection(): { status: boolean, rowIndexes?: number[], colIndexes?: number[] } {
        const gridObj: SfGrid = this.parent;
        let rowCellIndxes: ISelectedCell[];
        let obj: { status: boolean, rowIndexes?: number[], colIndexes?: number[] } = { status: false };
        if (gridObj.options.selectionMode === 'Cell') {
            rowCellIndxes = this.getSelectedRowCellIndexes();
            let str: string;
            const rowIndexes: number[] = [];
            let i: number;
            for (i = 0; i < rowCellIndxes.length; i++) {
                if (rowCellIndxes[parseInt(i.toString(), 10)].cellIndexes.length) {
                    rowIndexes.push(rowCellIndxes[parseInt(i.toString(), 10)].rowIndex);
                }
                if (rowCellIndxes[parseInt(i.toString(), 10)].cellIndexes.length) {
                    if (!str) {
                        str = JSON.stringify(rowCellIndxes[parseInt(i.toString(), 10)].cellIndexes.sort());
                    }
                    if (str !== JSON.stringify(rowCellIndxes[parseInt(i.toString(), 10)].cellIndexes.sort())) {
                        break;
                    }
                }
            }
            rowIndexes.sort((a: number, b: number) => { return a - b; });
            if (i > 0 && i === rowCellIndxes.length) {
                obj = { status: true, rowIndexes: rowIndexes, colIndexes: rowCellIndxes[0].cellIndexes };
            }
        }
        return obj;
    }
}
