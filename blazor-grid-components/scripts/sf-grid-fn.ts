import { BlazorDotnetObject, extend, isNullOrUndefined, print } from '@syncfusion/ej2-base';
import { EventHandler, MouseEventArgs, KeyboardEventArgs, KeyboardEvents, closest, Browser } from '@syncfusion/ej2-base';
import { getScrollableParent } from '@syncfusion/ej2-popups';
import { Scroll } from './scroll';
import { Freeze } from './freeze';
import { BlazorGridElement, IGridOptions, ScrollPositionType, InitModulesResults } from './interfaces';
import { iterateArrayOrObject, parentsUntil, getRowHeight, Global, getScrollBarWidth, getSiblingsHeight } from './util';
import { ColumnWidthService } from './width-controller';
import { HeaderDragDrop } from './header-drag-drop';
import { ContentDragDrop } from './content-drag-drop';
import { Reorder } from './reorder';
import { Resize } from './resize';
import { Group } from './group';
import { ColumnChooser } from './column-chooser';
import { ColumnMenu } from './column-menu';
import { Filter } from './filter';
import { Edit } from './edit';
import { Clipboard } from './clipboard';
import { CustomToolTip } from './tooltip';
import { RowDD } from './row-reorder';
import { Selection } from './selection';
import { VirtualHeaderRenderer, VirtualContentRenderer } from './virtual-scroll';
import { FrozenDD } from './frozen-drag-drop';
import { Column } from './interfaces';
import { InfiniteScroll } from './infinite-scroll';

/**
 * Constructor for the SfGrid client component.
 *
 * @param {HTMLElement} element - The root HTML element representing the grid.
 * @param {BlazorDotnetObject} dotNetRef - The Blazor .NET object reference for interop.
 * @param {string} dataId - The identifier for grid data.
 * @param {IGridOptions} options - Options to configure the grid behavior.
 * @param {HTMLElement} header - The HTML element representing the grid header.
 * @param {HTMLElement} content - The HTML element representing the grid content area.
 * @param {HTMLElement} footer - The HTML element representing the grid footer.
 */

export class SfGrid {
    public element: HTMLElement;
    public dotNetRef: BlazorDotnetObject;
    public dataId: string;
    public options: IGridOptions;
    public header: HTMLElement;
    public content: HTMLElement;
    public footer: HTMLElement;
    public columnModel: Column[] = [];
    public frozenColumnModel: Column[] = [];
    public scrollModule: Scroll;
    public freezeModule: Freeze;
    public headerDragDrop: HeaderDragDrop;
    public contentDragDrop: ContentDragDrop;
    public reorderModule: Reorder;
    public groupModule: Group;
    public columnChooserModule: ColumnChooser;
    public columnMenuModule: ColumnMenu;
    public filterModule: Filter;
    public resizeModule: Resize;
    public frozenDragDropModule: FrozenDD;
    public editModule: Edit;
    public clipboardModule: Clipboard;
    public virtualHeaderModule: VirtualHeaderRenderer;
    public virtualContentModule: VirtualContentRenderer;
    public keyModule: KeyboardEvents;
    public infiniteScrollModule: InfiniteScroll;
    private toolTipModule: CustomToolTip;
    public rowDragAndDropModule: RowDD;
    public selectionModule: Selection;
    public initModulesResults: InitModulesResults;
    private widthService: ColumnWidthService;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    private sfBlazor: any = (window as any).sfBlazor;
    private editedCellIndex: number = null;
    private isDeferredInitialization: boolean = false;
    private virtualRowHeightValue: number = null;
    private domVirtualScrollCleanup: () => void = null;
    private domVirtualMaxHeightCleanup: () => void = null;

    private firstFocusableTemplateElement: Element = null;
    private lastFocusableTemplateElement: Element = null;
    private stackedColumn: Column;
    private inViewIndexes: number[] = [];
    /** @hidden */
    public scrollPosition: ScrollPositionType;
    private isRendered: boolean = false;
    private isGridFirstRender: boolean = false;
    public isResetDataTriggered: boolean = false;
    public nColumnOffsets: number[] = [];
    public previousTarget: HTMLElement | null = null;
    private delegateClickHandler: (...args: unknown[]) => void;
    private delegateKeyDownHandler: (...args: unknown[]) => void;
    private preventSaveCellOnDragRelease: boolean = false;
    private _mouseDownX: number = null;
    private _mouseDownY: number = null;

    constructor(dataId: string, element: HTMLElement, options: IGridOptions, dotnetRef: BlazorDotnetObject) {
        this.element = element;
        if (isNullOrUndefined(this.element)) { return; }
        this.dotNetRef = dotnetRef;
        this.dataId = dataId;
        this.options = options;
        this.header = this.element.querySelector('.e-headercontent');
        this.content = this.element.querySelector('.e-gridcontent .e-content');
        this.footer = this.element.querySelector('.e-summarycontent');
        if (this.element.offsetWidth <= 0) {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const gridtimer: any = setInterval(() => {
                if (this.element.offsetWidth > 0) {
                    this.isDeferredInitialization = true;
                    this.initModulesResults = this.initModules();
                    clearInterval(gridtimer);
                }
            }, 500);
        }
        else {
            this.initModulesResults = this.initModules();
        }
        this.addScrollEvents(true);
        if (!isNullOrUndefined(this.element)) {
            (this.element as BlazorGridElement).blazor__instance = this;
            this.sfBlazor.setCompInstance(this);
        }
    }

    public getInitModulesResults(): InitModulesResults {
        if (!isNullOrUndefined(this.initModulesResults)) {
            this.initModulesResults.isMacDevice = navigator.userAgent.indexOf('Mac OS') !== -1;
        }
        return this.initModulesResults;
    }

    public initModules(): InitModulesResults {
        this.scrollModule = new Scroll(this);
        this.infiniteScrollModule = new InfiniteScroll(this);
        this.freezeModule = new Freeze(this);
        this.headerDragDrop = new HeaderDragDrop(this);
        this.contentDragDrop = new ContentDragDrop(this);
        this.reorderModule = new Reorder(this);
        this.groupModule = new Group(this);
        this.resizeModule = new Resize(this);
        this.frozenDragDropModule = new FrozenDD(this);
        this.editModule = new Edit(this);
        this.columnChooserModule = new ColumnChooser(this);
        this.clipboardModule = new Clipboard(this);
        this.columnMenuModule = new ColumnMenu(this);
        this.filterModule = new Filter(this);
        this.virtualContentModule = new VirtualContentRenderer(this);
        this.virtualHeaderModule = new VirtualHeaderRenderer(this);
        this.toolTipModule = new CustomToolTip(this);
        this.rowDragAndDropModule = new RowDD(this);
        this.selectionModule = new Selection(this);
        this.widthService = new ColumnWidthService(this);
        this.isRendered = this.options.isPrerendered;
        this.keyModule = new KeyboardEvents(
            this.element,
            {
                keyAction: this.keyActionHandler.bind(this),
                keyConfigs: gridKeyConfigs,
                eventName: 'keydown'
            }
        );
        let contentReadyResults: InitModulesResults;
        if (this.options.enableColumnVirtualization) {
            this.virtualHeaderModule.renderTable();
        }
        if (this.options.enableVirtualization || this.options.enableColumnVirtualization) {
            this.virtualRowHeightValue = this.virtualContentModule.renderTable();
        }
        if (this.options.enableDomVirtualization) {
            this.virtualRowHeightValue = this.getRowHeight();
            this.bindDomVirtualScroll();
            this.bindDomVirtualMaxHeight();
        }
        if (this.options.allowResizing) {
            this.resizeModule.render();
        }
        if (this.options.isFreezeLineMoved) {
            this.freezeLineMovedAction();
        }

        // needClientAction should only be used for virtual scroll and hideAtMedia features
        if (!this.options.needClientAction) {
            contentReadyResults = this.contentReady();
        } else {
            contentReadyResults = this.clientActions();
        }
        this.lastRowBorderCheck();
        this.wireEvents();

        if (!this.options.enableColumnVirtualization) {
            this.updateColumnWidth(this.options.columns);
        }
        if (contentReadyResults != null) {
            contentReadyResults.rowHeight = this.virtualRowHeightValue;
        }
        return contentReadyResults;
    }

    /**
     * Keeps the DOM virtualization custom scrollbar and `.e-content` in sync entirely on the client
     * and notifies .NET (at most once per animation frame, and never while a previous call is pending)
     * with the latest scrollTop so the visible row window can be re-rendered.
     *
     * @private
     * @returns {void}
     */
    private bindDomVirtualScroll(): void {
        if (this.domVirtualScrollCleanup) {
            this.domVirtualScrollCleanup();
        }
        const content: HTMLElement = this.getContent();
        const scrollbar: HTMLElement = this.element.querySelector('.e-dom-virtual-scrollbar') as HTMLElement;
        if (isNullOrUndefined(content) || isNullOrUndefined(scrollbar) || isNullOrUndefined(this.dotNetRef)) { return; }
        let lastSent: number = -1;
        let inFlight: boolean = false;
        let dirty: boolean = false;
        let disposed: boolean = false;
        let rafId: number = 0;
        const notify: () => void = (): void => {
            if (disposed) { return; }
            if (inFlight) {
                dirty = true;
                return;
            }
            const top: number = Math.round(content.scrollTop);
            if (top === lastSent) { return; }
            lastSent = top;
            inFlight = true;
            dirty = false;
            const done: () => void = (): void => {
                inFlight = false;
                if (dirty) { notify(); }
            };
            (this.dotNetRef.invokeMethodAsync('DomVirtualScroll', top) as unknown as Promise<void>).then(done, done);
        };
        const schedule: () => void = (): void => {
            if (rafId) { return; }
            rafId = requestAnimationFrame((): void => {
                rafId = 0;
                notify();
            });
        };
        const onContentScroll: () => void = (): void => {
            if (Math.abs(scrollbar.scrollTop - content.scrollTop) >= 1) {
                scrollbar.scrollTop = content.scrollTop;
            }
            schedule();
        };
        const onScrollbarScroll: () => void = (): void => {
            if (Math.abs(content.scrollTop - scrollbar.scrollTop) >= 1) {
                content.scrollTop = scrollbar.scrollTop;
            }
            schedule();
        };
        EventHandler.add(content, 'scroll', onContentScroll, this);
        EventHandler.add(scrollbar, 'scroll', onScrollbarScroll, this);
        this.domVirtualScrollCleanup = (): void => {
            disposed = true;
            if (rafId) { cancelAnimationFrame(rafId); }
            EventHandler.remove(content, 'scroll', onContentScroll);
            EventHandler.remove(scrollbar, 'scroll', onScrollbarScroll);
            this.domVirtualScrollCleanup = null;
        };
    }

    private getBrowserMaxScrollHeight(): number {
        const host: HTMLDivElement = document.createElement('div');
        const child: HTMLDivElement = document.createElement('div');
        host.style.position = 'absolute';
        host.style.visibility = 'hidden';
        host.style.height = '1px';
        host.style.width = '1px';
        host.style.overflow = 'scroll';
        host.style.top = '-10000px';
        child.style.height = '1000000000px';
        host.appendChild(child);
        document.body.appendChild(host);
        const measured: number = Math.max(child.offsetHeight, host.scrollHeight);
        document.body.removeChild(host);
        return measured > 0 ? measured : 32767000;
    }

    private bindDomVirtualMaxHeight(): void {
        if (this.domVirtualMaxHeightCleanup) {
            this.domVirtualMaxHeightCleanup();
        }
        if (isNullOrUndefined(this.dotNetRef)) { return; }
        let disposed: boolean = false;
        let rafId: number = 0;
        let lastHeight: number = -1;
        const notify: () => void = (): void => {
            if (disposed) { return; }
            const current: number = this.getBrowserMaxScrollHeight();
            if (current <= 0 || current === lastHeight) { return; }
            lastHeight = current;
            this.dotNetRef.invokeMethodAsync('SetDomVirtualMaxHeight', current);
        };
        const schedule: () => void = (): void => {
            if (rafId) { return; }
            rafId = requestAnimationFrame((): void => {
                rafId = 0;
                notify();
            });
        };
        notify();
        window.addEventListener('resize', schedule);
        if (window.visualViewport) {
            window.visualViewport.addEventListener('resize', schedule);
        }
        this.domVirtualMaxHeightCleanup = (): void => {
            disposed = true;
            if (rafId) { cancelAnimationFrame(rafId); }
            window.removeEventListener('resize', schedule);
            if (window.visualViewport) {
                window.visualViewport.removeEventListener('resize', schedule);
            }
            this.domVirtualMaxHeightCleanup = null;
        };
    }

    public getHeaderContent(): HTMLElement { return this.header; }
    public getHeaderTable(): Element { return this.header.querySelector('.e-table'); }
    public getContent(): HTMLElement { return this.content; }
    public getContentTable(): Element { return this.content.querySelector('.e-table'); }
    public getFooterContent(): HTMLElement { return this.footer; }

    public getColumns(autoFitVirtual: boolean = false): Column[] {
        // let inview: number[] = this.inViewIndexes.map((v: number) => v - this.groupSettings.columns.length).filter((v: number) => v > -1);
        // let vLen: number = inview.length;
        // if (!this.enableColumnVirtualization || isNullOrUndefined(this.columnModel) || this.columnModel.length === 0 || isRefresh) {
        //     this.columnModel = [];
        //     this.updateColumnModel(this.columns as Column[]);
        // }
        // let columns: Column[] = vLen === 0 ? this.columnModel :
        //     this.columnModel.slice(inview[0], inview[vLen - 1] + 1);
        this.columnModel = [];
        let columns: Column[] = this.options.enableColumnVirtualization && autoFitVirtual ?
            this.options.virtualizedColumns as Column[] : this.options.columns as Column[];
        if (autoFitVirtual && this.options.frozenColumns > 0) {
            columns = this.getOrderedFrozenColumns();
            this.columnModel = columns;
        }
        else {
            this.updateColumnModel(columns);
        }
        return this.columnModel;
    }

    public getOrderedFrozenColumns(): Column[] {
        const columns: Column[] = [];
        const gridColumns: Column[] = this.getColumns();
        gridColumns.filter((c: Column) => c.isFrozen && c.freeze === 'Left')
            .map((c: Column) => columns.push(c));
        gridColumns.filter((c: Column) => !c.isFrozen || (c.isFrozen && c.freeze === 'Fixed'))
            .map((c: Column) => columns.push(c));
        gridColumns.filter((c: Column) => c.isFrozen && c.freeze === 'Right')
            .map((c: Column) => columns.push(c));
        return columns;
    }

    public autofitFrozenColumns(autofitAllColumns?: boolean): string[] {
        const columns: string[] = [];
        const gridColumns: Column[] = this.getColumns();
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const left: number[] = gridColumns.filter((c: Column) => (autofitAllColumns || c.autoFit) && c.isFrozen && c.freeze === 'Left')
            .map((c: Column) => columns.push(c.field || c.uid));
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const movable: number[] = gridColumns.filter((c: Column) => (autofitAllColumns || c.autoFit) && !c.isFrozen)
            .map((c: Column) => columns.push(c.field || c.uid));
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const right: number[] = gridColumns.filter((c: Column) => (autofitAllColumns || c.autoFit) && c.isFrozen && c.freeze === 'Right')
            .map((c: Column) => columns.push(c.field || c.uid));
        return columns;
    }

    public freezeLineMovedAction(): void {
        this.options.isFreezeLineMoved = false;
        let movableContent: HTMLElement = this.getContent().querySelector('.e-movablecontent');
        if (isNullOrUndefined(movableContent)) {
            movableContent = this.getContent();
        }
        const movableTable: HTMLElement = movableContent.querySelector('table');
        if (!isNullOrUndefined(movableTable) && movableTable.style.width !== '') {
            if (this.options.frozenLeftColumnsCount !== 0 || this.options.frozenRightColumnsCount !== 0) {
                this.updateColumnLevelFrozen();
            }
            const widthService: ColumnWidthService = new ColumnWidthService(this);
            widthService.setWidthToTable();
        }
        movableContent.scrollLeft = 0;
        if (!isNullOrUndefined(this.element.querySelector('.e-movablescrollbar'))) {
            this.element.querySelector('.e-movablescrollbar').scrollLeft = 0;
        }

    }

    private addScrollEvents(add: boolean): void {
        if (this.options.showColumnMenu) {
            const elements: HTMLElement[] = getScrollableParent(this.element);
            for (let i: number = 0; !isNullOrUndefined(elements) && i < elements.length; i++) {
                if (elements[parseInt(i.toString(), 10)] instanceof HTMLElement) {
                    if (add) {
                        EventHandler.add(elements[parseInt(i.toString(), 10)], 'scroll', this.scrollHandler, this);
                    } else {
                        EventHandler.remove(elements[parseInt(i.toString(), 10)], 'scroll', this.scrollHandler);
                    }
                }
            }
            if (!isNullOrUndefined(this.content)) {
                if (add) {
                    EventHandler.add(this.content, 'scroll', this.scrollHandler, this);
                } else {
                    EventHandler.remove(this.content, 'scroll', this.scrollHandler);
                }
            }
        }
    }

    private scrollHandler(): void {
        if (!isNullOrUndefined(this.element) && !isNullOrUndefined((this.element as BlazorGridElement).blazor__instance)
            && !isNullOrUndefined(this.columnMenuModule)) {
            return this.columnMenuModule.setPosition();
        }
    }

    public updateColumnLevelFrozen(): void {
        let cols: Column[] = this.columnModel;
        if (this.options.enableColumnVirtualization) {
            cols = cols.filter((x: Column) => x.visible);
        }
        const leftCols: Column[] = []; const rightCols: Column[] = []; const movableCols: Column[] = [];
        if (this.options.frozenRightCount !== 0 || this.options.frozenLeftCount !== 0 || this.options.frozenColumns !== 0) {
            for (let i: number = 0, len: number = cols.length; i < len; i++) {
                const col: Column = cols[parseInt(i.toString(), 10)];
                if (col.freeze === 'Left' && col.isFrozen || col.index < this.options.frozenColumns) {
                    leftCols.push(col);
                }
                else if (col.freeze === 'Right' && col.isFrozen) {
                    rightCols.push(col);
                }
                else {
                    movableCols.push(col);
                }
            }
            this.frozenColumnModel = leftCols.concat(movableCols).concat(rightCols);
        }
    }

    private updateColumnModel(columns: Column[]): void {
        if (!isNullOrUndefined(columns)) {
            for (let i: number = 0, len: number = columns.length; i < len; i++) {
                if (columns[parseInt(i.toString(), 10)].columns != null &&
                    (columns[parseInt(i.toString(), 10)].columns as Column[]).length > 0) {
                    this.updateColumnModel(columns[parseInt(i.toString(), 10)].columns as Column[]);
                } else {
                    this.columnModel.push(columns[parseInt(i.toString(), 10)] as Column);
                }
            }
            this.updateFixedcolumns();
        }
    }
    public updateColumnWidth(columns: Column[]): void {
        this.nColumnOffsets = [];
        let offset: number = 0;
        if (!this.options.enableColumnVirtualization && !isNullOrUndefined(columns)) {
            for (let i: number = 0, len: number = columns.length; i < len; i++) {
                offset = parseInt(offset.toString(), 10) + (columns[parseInt(i.toString(), 10)].visible ?
                    parseInt(<string>columns[parseInt(i.toString(), 10)].width, 10) : 0);
                this.nColumnOffsets.push(offset);
            }
        }
    }

    public getColumnByIndex(index: number, frozenCols: boolean = false): Column {
        let column: Column;
        this.getColumns(frozenCols).some((col: Column, i: number) => {
            column = col;
            return i === index;
        });
        return column;
    }

    public getDataRows(): Element[] {
        const contentTable: Element = this.getContentTable();
        if (isNullOrUndefined(contentTable) || isNullOrUndefined(contentTable.querySelector('tbody'))) { return []; }
        let rows: HTMLElement[] = [].slice.call(contentTable.querySelector('tbody').children);
        if (this.options.frozenRows) {
            const headerTable: Element = this.getHeaderTable();
            const headerBody: Element = !isNullOrUndefined(headerTable) ? headerTable.querySelector('tbody') : null;
            if (!isNullOrUndefined(headerBody)) {
                const freezeRows: HTMLElement[] = [].slice.call(headerBody.children);
                rows = this.addMovableRows(freezeRows, rows);
            }
        }
        const dataRows: Element[] = this.generateDataRows(rows);
        return dataRows;
    }

    public addMovableRows(fRows: HTMLElement[], mrows: HTMLElement[]): HTMLElement[] {
        for (let i: number = 0, len: number = mrows.length; i < len; i++) {
            fRows.push(mrows[parseInt(i.toString(), 10)]);
        }
        return fRows;
    }

    private generateDataRows(rows: HTMLElement[]): Element[] {
        const dRows: Element[] = [];
        for (let i: number = 0, len: number = rows.length; i < len; i++) {
            if (rows[parseInt(i.toString(), 10)].classList.contains('e-row') && !rows[parseInt(i.toString(), 10)].classList.contains('e-hiddenrow')) {
                dRows.push(rows[parseInt(i.toString(), 10)] as Element);
            }
        }
        return dRows;
    }

    public getMovableDataRows(): Element[] {
        const contentBody: Element = this.getContent().querySelector('tbody');
        if (isNullOrUndefined(contentBody)) { return []; }
        let rows: HTMLElement[] = [].slice.call(contentBody.children);
        if (this.options.frozenRows) {
            const headerBody: Element = this.getHeaderContent().querySelector('tbody');
            if (!isNullOrUndefined(headerBody)) {
                const freezeRows: HTMLElement[] = [].slice.call(headerBody.children);
                rows = this.addMovableRows(freezeRows, rows);
            }
        }
        const dataRows: Element[] = this.generateDataRows(rows);
        return dataRows;
    }

    public getFrozenDataRows(): Element[] {
        const frozenContent: Element = this.getContent().querySelector('.e-frozencontent');
        const frozenBody: Element = !isNullOrUndefined(frozenContent) ? frozenContent.querySelector('tbody') : null;
        if (isNullOrUndefined(frozenBody)) { return []; }
        let rows: HTMLElement[] = [].slice.call(frozenBody.children);
        if (this.options.frozenRows) {
            const frozenHeader: Element = this.getHeaderContent().querySelector('.e-frozenheader');
            const frozenHeaderBody: Element = !isNullOrUndefined(frozenHeader) ? frozenHeader.querySelector('tbody') : null;
            if (!isNullOrUndefined(frozenHeaderBody)) {
                const freezeRows: HTMLElement[] = [].slice.call(frozenHeaderBody.children);
                rows = this.addMovableRows(freezeRows, rows);
            }
        }
        const dataRows: Element[] = this.generateDataRows(rows);
        return dataRows;
    }

    public leftrightColumnWidth(position?: string): number {
        const cols: Column[] = position === 'left' ? this.getFrozenLeftColumn() : position === 'right' ? this.getFrozenRightColumns() : [];
        let width: number = 0;
        cols.filter((col: Column) => {
            if (col.visible && !isNullOrUndefined(col.width)) {
                width += parseInt(col.width.toString(), 10);
            }
        });
        return width;
    }

    public getFrozenLeftColumn = (): Column[] => {
        const columns: Column[] = [];
        const gridColumns: Column[] = this.getColumns();
        gridColumns.filter((c: Column) => c.isFrozen && c.freeze === 'Left')
            .map((c: Column) => columns.push(c));
        return columns;
    }

    public getFrozenRightColumns = (): Column[] => {
        const columns: Column[] = [];
        const gridColumns: Column[] = this.getColumns();
        gridColumns.filter((c: Column) => c.isFrozen && c.freeze === 'Right')
            .map((c: Column) => columns.push(c));
        return columns;
    }

    public getFrozenRightDataRows(): Element[] {
        const frozenRightContent: Element = this.getContent().querySelector('.e-frozen-right-content');
        const frozenRightBody: Element = !isNullOrUndefined(frozenRightContent) ? frozenRightContent.querySelector('tbody') : null;
        if (isNullOrUndefined(frozenRightBody)) { return []; }
        let rows: HTMLElement[] = [].slice.call(frozenRightBody.children);
        if (this.options.frozenRows) {
            const frozenHeader: Element = this.getHeaderContent().querySelector('.e-frozenheader');
            const frozenHeaderBody: Element = !isNullOrUndefined(frozenHeader) ? frozenHeader.querySelector('tbody') : null;
            if (!isNullOrUndefined(frozenHeaderBody)) {
                const freezeRows: HTMLElement[] = [].slice.call(frozenHeaderBody.children);
                rows = this.addMovableRows(freezeRows, rows);
            }
        }
        return this.generateDataRows(rows);
    }

    public getRowByIndex(index: number): Element {
        return this.getDataRows()[parseInt(index.toString(), 10)];
    }

    public getCellFromIndex(rowIndex: number, columnIndex: number): Element {
        return this.getDataRows()[parseInt(rowIndex.toString(), 10)] && this.getDataRows()[parseInt(rowIndex.toString(), 10)].querySelectorAll('.e-rowcell')[parseInt(columnIndex.toString(), 10)];
    }

    public isMovableGrid(index: number, frozenColumns: boolean): boolean {
        const gridColumns: Column[] = this.getColumns(frozenColumns);
        if (this.options.actualFrozenColumns > 0) {
            return index >= this.options.actualFrozenColumns;
        }
        else {
            const isFrozenColumnsIndexes: number[] = [];
            gridColumns.forEach((col: Column) => { if (col.isFrozen) { isFrozenColumnsIndexes.push(col.index); } });
            return isFrozenColumnsIndexes.indexOf(index) === -1;
        }
    }

    public getColumnHeaderByIndex(index: number): Element {
        return this.getHeaderTable().querySelectorAll('.e-headercell')[parseInt(index.toString(), 10)];
    }

    public getRows(): Element[] {
        let dataRows: HTMLElement[] = [].slice.call(this.getContentTable().querySelectorAll('tr.e-row[data-uid]'));
        if (this.options.frozenRows) {
            const freezeRows: HTMLElement[] =
                [].slice.call(this.getHeaderContent().querySelectorAll('tr.e-row[data-uid]'));
            dataRows = this.addMovableRows(freezeRows, dataRows);
        }
        return dataRows as Element[];
    }

    public getSelectedRows(): Element[] {
        return this.getRows().filter((row: Element) => row.getAttribute('aria-selected') === 'true');
    }

    public getSelectedRowIndexes(isVirtualScroll?: boolean): number[] {
        const selectedIndexes: number[] = [];
        const rows: Element[] = this.getRows();
        for (let i: number = 0; i < rows.length; i++) {
            if (rows[parseInt(i.toString(), 10)].hasAttribute('aria-selected') && rows[parseInt(i.toString(), 10)].getAttribute('aria-selected') === 'true') {
                const rowIndex: number = ((this.options.allowDragSelection && this.options.enableVirtualization) || isVirtualScroll) ? parseInt(rows[parseInt(i.toString(), 10)].getAttribute('aria-rowindex'), 10) - 1 : i;
                selectedIndexes.push(rowIndex);
            }
        }
        return selectedIndexes;
    }

    public getVisibleColumns(): Column[] {
        const cols: Column[] = [];
        for (const col of this.columnModel) {
            if (col.visible) {
                cols.push(col);
            }
        }
        return cols;
    }
    /**
     * Gets a Column by column name.
     *
     * @param  {string} field - Specifies the column name.
     * @returns {Column} The column object that matches the given field name.
     * @blazorType GridColumn
     */
    public getColumnByField(field: string): Column {
        return iterateArrayOrObject<Column, Column>(<Column[]>this.getColumns(), (item: Column) => {
            if (item.field === field) {
                return item;
            }
            return undefined;
        })[0];
    }

    /**
     * Gets a column index by column name.
     *
     * @param  {string} field - Specifies the column name.
     * @param {boolean} [virtualAutoFit=false] - Specifies whether to consider virtual autofit.
     * @returns {number} The index of the column that matches the given field name, or -1 if not found.
     */
    public getColumnIndexByField(field: string, virtualAutoFit: boolean = false): number {
        const cols: Column[] = this.getColumns(virtualAutoFit);
        for (let i: number = 0; i < cols.length; i++) {
            if (cols[parseInt(i.toString(), 10)].field === field) {
                return i;
            }
        }
        return -1;
    }

    /**
     * Gets a column by UID.
     *
     * @param  {string} uid - Specifies the column UID.
     * @returns {Column} The column object that matches the given UID.
     * @blazorType GridColumn
     */
    public getColumnByUid(uid: string): Column {
        return iterateArrayOrObject<Column, Column>(
            [...<Column[]>this.getColumns(), ...this.getStackedColumns(this.options.columns as Column[])],
            (item: Column) => {
                if (item.uid === uid) {
                    return item;
                }
                return undefined;
            })[0];
    }

    /**
     * @hidden
     * Recursively gets all stacked columns.
     *
     * @param {Column[]} columns - The array of columns to search through.
     * @param {Column[]} [stackedColumn=[]] - The array to store stacked columns.
     * @returns {Column[]} An array of stacked columns.
     */
    public getStackedColumns(columns: Column[], stackedColumn: Column[] = []): Column[] {
        for (const column of columns) {
            if (column.columns) {
                stackedColumn.push(column);
                this.getStackedColumns(column.columns as Column[], stackedColumn);
            }
        }
        return stackedColumn;
    }

    /**
     * Gets a column index by UID.
     *
     * @param {string} uid - Specifies the column UID.
     * @param {boolean} [virtualAutoFit=false] - Specifies whether to consider virtual autofit.
     * @returns {number} The index of the column that matches the given UID, or -1 if not found.
     */
    public getColumnIndexByUid(uid: string, virtualAutoFit: boolean = false): number {
        const index: number = iterateArrayOrObject<number, Column>
        (<Column[]>this.getColumns(virtualAutoFit), (item: Column, index: number) => {
            if (item.uid === uid) {
                return index;
            }
            return undefined;
        })[0];

        return !isNullOrUndefined(index) ? index : -1;
    }

    /**
     * Retrieves the column header element by its UID.
     *
     * @param {string} uid - The UID of the column to retrieve.
     * @returns {Element | null} The column header element that matches the given UID, or null if not found.
     */
    public getColumnHeaderByUid(uid: string): Element | null {
        const headerCell: Element = this.getHeaderContent().querySelector('[e-mappinguid=' + uid + ']') as Element;
        const headerContainer: HTMLElement | null = headerCell ? headerCell.parentElement as HTMLElement : null;
        return headerContainer ? (headerContainer.parentElement) as Element : null;
    }

    /**
     * Gets UID by column name.
     *
     * @param {string} field - Specifies the column name.
     * @param {boolean} [virtualAutoFit=false] - Specifies whether to consider virtual autofit.
     * @returns {string} The UID of the column that matches the given field name.
     */
    public getUidByColumnField(field: string, virtualAutoFit: boolean = false): string {
        return iterateArrayOrObject<string, Column>(<Column[]>this.getColumns(virtualAutoFit), (item: Column) => {
            if (item.field === field) {
                return item.uid;
            }
            return undefined;
        })[0];
    }

    public getStackedHeaderColumnByHeaderText(stackedHeader: string, col: Column[]): Column {
        for (let i: number = 0; i < col.length; i++) {
            const individualColumn: Column = col[parseInt(i.toString(), 10)];
            if (individualColumn.field === stackedHeader || individualColumn.headerText === stackedHeader) {
                this.stackedColumn = individualColumn;
                break;
            } else if (individualColumn.columns) {
                this.getStackedHeaderColumnByHeaderText(stackedHeader, <Column[]>individualColumn.columns);
            }
        }
        return this.stackedColumn;
    }

    /**
     * Gets TH index by column uid value.
     *
     * @private
     * @param  {string} uid - Specifies the column uid.
     * @param {boolean} [virtualAutoFit=false] - Specifies whether to consider virtual autofit.
     * @returns {number} he normalized column index.
     */
    public getNormalizedColumnIndex(uid: string, virtualAutoFit: boolean = false): number {
        const index: number = this.getColumnIndexByUid(uid, virtualAutoFit);
        return index + this.getIndentCount();
    }

    /**
     * Gets indent cell count.
     *
     * @private
     * @returns {number} The count of indent cells.
     */
    public getIndentCount(): number {
        let index: number = 0;
        if (this.options.allowGrouping) {
            index += this.options.groupCount;
        }
        if (this.options.hasDetailTemplate) {
            index++;
        }
        if (this.options.allowRowDragAndDrop && !this.options.hasDropTarget) {
            index++;
        }
        /**
         * TODO: index normalization based on the stacked header, grouping and detailTemplate
         * and frozen should be handled here
         */
        return index;
    }

    public isPercentageWidth(): boolean {

        const columns: Column[] = this.getVisibleColumns();
        let percentageCol: number = 0;
        let undefinedWidthCol: number = 0;

        for (let i: number = 0; i < columns.length; i++) {

            if (isNullOrUndefined(columns[parseInt(i.toString(), 10)].width)) {
                undefinedWidthCol++;
            } else if (columns[parseInt(i.toString(), 10)].width.toString().indexOf('%') !== -1) {
                percentageCol++;
            }
        }
        return percentageCol === columns.length && !undefinedWidthCol;
    }

    /**
     * Gets indent Cell Width
     *
     * @hidden
     * @returns {void}
     */
    public recalcIndentWidth(): {isRowDragCellEnable: boolean | null; indentWidthCalc: string | null} {
        if ((this.options.isRenderedFromTreeGrid && this.options.hasDetailTemplate && this.options.allowRowDragAndDrop) || (!this.isRendered || !this.getHeaderTable().querySelector('.e-emptycell'))) {
            return{
                isRowDragCellEnable: null,
                indentWidthCalc: null
            };
        }
        const emptyCells: NodeListOf<Element> = this.getHeaderTable().querySelectorAll('.e-emptycell');
        const detailIndentCell: Element[] = Array.from(emptyCells).filter((cell: Element) => cell.parentElement.classList.contains('e-detailheadercell'));
        const isRowDragHeaderCell: Element[] = Array.from(emptyCells).filter((cell: Element) => cell.parentElement.classList.contains('e-rowdragheader'));
        const groupingIndentCell: Element[] = Array.from(emptyCells).filter((cell: Element) => cell.parentElement.classList.contains('e-grouptopleftcell'));
        const requestIsNotRefreshOrGrouping: boolean = !isNullOrUndefined(this.options.requestType) && this.options.requestType !== 'Refresh'
            && this.options.requestType !== 'Grouping' && this.options.requestType !== 'UnGrouping';
        let detailIndentRefresh: string = 'false';
        let groupIndentRefresh: string = 'false';
        if (detailIndentCell.length > 0) {
            detailIndentRefresh = detailIndentCell[0].classList.contains('e-indentRefreshed').toString();
        }
        if (groupingIndentCell.length > 0) {
            groupIndentRefresh = groupingIndentCell[0].classList.contains('e-indentRefreshed').toString();
        }
        // Handle Detail and DragDrop
        if ((!this.options.groupCount && !this.options.hasDetailTemplate &&
            (this.options.allowRowDragAndDrop && this.options.hasDropTarget)) || !this.getContentTable() ||
            isRowDragHeaderCell.length === 0 && (detailIndentRefresh === 'true' || groupIndentRefresh === 'true')) {
            return{
                isRowDragCellEnable: null,
                indentWidthCalc: null
            };
        }
        if ((requestIsNotRefreshOrGrouping && groupIndentRefresh === 'true') ||
            (isRowDragHeaderCell.length > 0 && isRowDragHeaderCell[0].classList.contains('e-indentRefreshed'))) {
            return{
                isRowDragCellEnable: null,
                indentWidthCalc: null
            };
        }

        const emptyCellElement: HTMLElement = (this.getHeaderTable().querySelector('.e-emptycell').parentElement as HTMLElement);
        let indentWidth: number = isNullOrUndefined(emptyCellElement) ? 0 : emptyCellElement.offsetWidth;
        
        // Applies the default indent cell offsetWidth value when grid is rendered inside
        // the tab component which is not currently visible (ContentLoad.Init).
        // Refer BLAZ-1032782 for more details.
        if (indentWidth === 0 && parentsUntil(emptyCellElement, 'e-tab')) {
            return {
                isRowDragCellEnable: null,
                indentWidthCalc: null
            };
        }
        
        if (isRowDragHeaderCell.length > 0 && groupingIndentCell.length > 0 && groupIndentRefresh === 'false') {
            indentWidth = groupingIndentCell[0].parentElement.offsetWidth;
        }
        if (isRowDragHeaderCell.length > 0 && (groupingIndentCell.length === 0 || groupIndentRefresh === 'true')) {
            indentWidth = isRowDragHeaderCell[0].parentElement.offsetWidth;
        }
        const perPixel: number = indentWidth / 30;
        if (perPixel >= 1) {
            indentWidth = (30 / perPixel);
        }
        this.getHeaderTable().querySelector('.e-emptycell').classList.add('e-indentRefreshed');
        if (isRowDragHeaderCell.length > 0) {
            isRowDragHeaderCell[0].classList.add('e-indentRefreshed');
        }
        const isRowDragCell: boolean = Array.from(this.getHeaderTable().querySelectorAll('.e-emptycell'))
            .some((x: Element) => x.parentElement.classList.contains('e-rowdragheader'));
        let calculatedIndentWidth: string = '';
        if (this.isPercentageWidth() && perPixel > 1) {
            const perPixel: number = indentWidth / 30;
            if (perPixel >= 1) {
                indentWidth = (30 / perPixel);
                indentWidth = indentWidth > 5 ? 3.5 : indentWidth;
            }
            calculatedIndentWidth = indentWidth + '%';
        }
        else {
            calculatedIndentWidth = indentWidth + 'px';
        }
        return {
            isRowDragCellEnable: isRowDragCell,
            indentWidthCalc: calculatedIndentWidth
        };
    }

    public resetColumnWidth(): void {
        if ((this.options.width === 'auto' || typeof (this.options.width) === 'string')
            && this.getColumns().filter((col: Column) => (!col.width || col.width === 'auto') && col.minWidth).length > 0) {
            const tgridWidth: number = this.widthService.getTableWidth(this.getColumns());
            this.widthService.setMinwidthBycalculation(tgridWidth);
        }
    }

    public getContentCell(element: HTMLElement, top: number, left: number): object {
        if (!isNullOrUndefined(element)) {
            let cellElement:  HTMLElement = document.elementFromPoint(left, top) as HTMLElement;
            let mappingUidDiv: HTMLElement = null;
            let mappingUid: string = null;
            let headerCellDiv: HTMLElement = null;
            if (!isNullOrUndefined(cellElement) && cellElement.getAttribute('aria-colIndex') === null) {
                const headerCell: HTMLElement = parentsUntil(cellElement, 'e-headercell') as HTMLElement;
                const rowCell: HTMLElement = parentsUntil(cellElement, 'e-rowcell') as HTMLElement;
                cellElement = headerCell !== null ? headerCell : (rowCell !== null ? rowCell : cellElement);
            }
            if (!isNullOrUndefined(cellElement)) {
                const rowCellIndex: HTMLElement = parentsUntil(cellElement, 'e-row') as HTMLElement;
                const colIndex: number = parseInt(cellElement.getAttribute('aria-colindex'), 10) - 1;
                const rowIndex: number = rowCellIndex ? parseInt(rowCellIndex.getAttribute('aria-rowindex'), 10) - 1 : -1;
                if (isNullOrUndefined(rowCellIndex)) {
                    mappingUidDiv = cellElement.querySelector('div[e-mappinguid]') as HTMLElement;
                    if (isNullOrUndefined(mappingUidDiv)) {
                        headerCellDiv = parentsUntil(cellElement, 'e-headercelldiv') as HTMLElement;
                        if (!isNullOrUndefined(headerCellDiv)) {
                            cellElement = headerCellDiv;
                            mappingUid = headerCellDiv.getAttribute('e-mappinguid');
                        }
                    } else {
                        mappingUid = mappingUidDiv.getAttribute('e-mappinguid');
                    }
                }
                return { ColumnIndex: colIndex , RowIndex: rowIndex , MappingUid: mappingUid };
            }
        }
    }

    public contentReady(action: string = null, isResetData?: boolean): InitModulesResults {

        //To add 100% width for main HTML element, when grid width is 100% or auto
        const mainLayoutTag: HTMLElement = document.getElementsByTagName('main')[0] as HTMLElement;
        const gridInitializeResults: InitModulesResults = {
            rowHeight: null,
            isMacDevice : null,
            indentWidth: null,
            isRowDragCell: null
        };
        if (!isNullOrUndefined(mainLayoutTag) && !isNullOrUndefined(mainLayoutTag.parentElement) && mainLayoutTag.parentElement.classList.contains('page') && (this.element.style.width === '100%' || 'auto')) {
            mainLayoutTag.style.width = '100%';
        }
        if (this.getColumns().some((x: Column) => x.autoFit)) {

            // Add a setTimeout function to auto-fit columns when using frozen columns with a custom adapter.
            // eslint-disable-next-line @typescript-eslint/no-this-alias
            const __this: this = this;
            if (__this.element.querySelector('.e-emptyrow') && __this.options.frozenColumns) {
                setTimeout(function (): void {
                    __this.resizeModule.autoFit();
                }, 100);
            }
            else {
                this.resizeModule.autoFit();
                this.freezeModule.updateFrozenColumnStyles();
            }
        }
        if (this.options.allowResizing && this.isGridFirstRender && this.options.isColumnResized) {
            const widthService: ColumnWidthService = new ColumnWidthService(this);
            widthService.setWidthToTable();
            this.isGridFirstRender = false;
        }
        if (!this.isGridFirstRender && this.options.frozenColumns && (this.options.enablePersistence || isResetData)) {
            const widthService: ColumnWidthService = new ColumnWidthService(this);
            widthService.setWidthToTable();
        }
        if (this.options.isColumnReordered && !isNullOrUndefined(this.getContent().querySelector('.e-movablecontent')) &&
            this.getContent().querySelector('.e-movablecontent').querySelector('table').style.width !== '') {
            const widthService: ColumnWidthService = new ColumnWidthService(this);
            widthService.setWidthToTable();
        }
        if (this.options.frozenColumns && this.options.enableColumnVirtualization) {
            this.freezeModule.setFrozenHeight();
            if (this.options.aggregatesCount !== 0) {
                const rowSummary: NodeListOf<HTMLElement> = this.element.querySelectorAll('.e-summaryrow');
                let height: number = 0;
                for (let i: number = 0; i < rowSummary.length; i++) {
                    if (rowSummary[parseInt(i.toString(), 10)].querySelectorAll('.e-templatecell').length > 0) {
                        height = rowSummary[parseInt(i.toString(), 10)].offsetHeight;
                        break;
                    }
                }
                for (let i: number = 0; i < rowSummary.length; i++) {
                    rowSummary[parseInt(i.toString(), 10)].style.height = height + 'px';
                }
            }
        }
        if (this.options.enableVirtualization || this.options.enableColumnVirtualization) {
            this.virtualContentModule.onDataReady();
        }
        const indentResults: { indentWidthCalc: string; isRowDragCellEnable: boolean } = this.recalcIndentWidth();
        if (this.isDeferredInitialization && !isNullOrUndefined(this.dotNetRef)) {
            this.dotNetRef.invokeMethodAsync('SetGridInitInfo', indentResults?.indentWidthCalc ?? null, indentResults?.isRowDragCellEnable ?? null,
                this.virtualRowHeightValue ?? null, navigator.userAgent.indexOf('Mac OS') !== -1);
            this.isDeferredInitialization = false;
        }
        if (!isNullOrUndefined(indentResults)) {
            gridInitializeResults.indentWidth = indentResults.indentWidthCalc;
            gridInitializeResults.isRowDragCell = indentResults.isRowDragCellEnable;
        }
        this.resetColumnWidth();
        this.lastRowBorderCheck();
        if (this.options.autoFit) {
            this.addTableBorderClass();
        }
        if (action === 'Paging') { //restore focus on paging.
            if (!parentsUntil(document.activeElement, 'e-grid')) {
                this.element.focus();
            }
        }
        if (this.options.enableStickyHeader) {
            this.scrollModule.addStickyListener(true);
            const groupElem: HTMLElement = this.element.querySelector('.e-groupdroparea') as HTMLElement;
            if (!isNullOrUndefined(groupElem) && groupElem.classList.contains('e-sticky') && groupElem.style.top === '') {
                groupElem.classList.remove('e-sticky');
            }
        }
        if (this.options.enableInfiniteScrolling) {
            this.infiniteScrollModule.infiniteOnDataReady();
            this.infiniteScrollModule.resetInfniniteScrollPositions();
        }
        if (!isNullOrUndefined(this.toolTipModule) && !isNullOrUndefined(this.toolTipModule.toolTipElement)) {
            this.toolTipModule.close();
        }
        const frozenRightColumns: Column[] = this.getColumns().filter((a: Column) => {
            return a.isFrozen && a.freeze === 'Right';
        });
        if (frozenRightColumns.length > 0) {
            //Below if condition checks that the grid contains horizontal scrollbar
            if (this.getContent().scrollWidth > this.getContent().clientWidth) {
                this.element.classList.add('e-right-shadow');
            }
        }
        const frozenRowBorder: HTMLElement = this.element.querySelector('.e-frozenrow-border') as HTMLElement;
        if (this.options.frozenRows > 0 && !isNullOrUndefined(frozenRowBorder)) {
            frozenRowBorder.style.width = this.getContent().scrollHeight > this.getContent().offsetHeight ? this.element.offsetWidth - 17 + 'px' : this.element.offsetWidth + 'px';
        }
        if ((this.options.width === '100%' || 'auto') && this.options.frozenColumns > 0 && this.options.allowGrouping && this.options.groupCount > 0 && !isNullOrUndefined(this.dotNetRef)) {
            const width: number = this.getContent().offsetWidth;
            this.dotNetRef.invokeMethodAsync('CalculateOffSetWidth', width);
        }
        if (this.options.frozenColumns > 0 && this.options.allowGrouping && this.options.groupCount > 0 && !isNullOrUndefined(this.dotNetRef)) {
            if (!(this.getContent().scrollWidth > this.getContent().clientWidth)) {
                this.dotNetRef.invokeMethodAsync('PreventExtraFrozenCellRendering');
            }
        }
        // Call height adjustment in vertical mode when text wrap is enabled
        if (this.options.enableAdaptiveUI && this.options.rowRenderingMode === 'Vertical' && this.options.allowTextWrap && (this.options.wrapMode === 'Header' || this.options.wrapMode === 'Both')) {
            this.adjustHeightsForTextWrap();
        }
        if (this.options.enableVirtualization && this.virtualContentModule.keyCombination === 'ctrlEnd') {
            const rows: NodeListOf<Element> = this.getContent().querySelectorAll('tr.e-row[data-uid]');
            if (!isNullOrUndefined(rows) && rows.length > 0) {
                const firstRow: Element = rows[0];
                const lastRow: Element = rows[rows.length - 1];
                const firstRowFirstCell: Element = firstRow.querySelector('td[tabindex="0"]');
                const lastRowLastCell: Element = lastRow.querySelector('td[tabindex="0"]');
                const allFocusableCells: Element[] = Array.from(
                    this.getContent().querySelectorAll('td[tabindex="0"]')
                );
                if (!isNullOrUndefined(allFocusableCells) && allFocusableCells.length > 0) {
                    const cellsToFocus: Element[] = allFocusableCells.filter((cell: Element) =>
                        cell !== firstRowFirstCell && cell !== lastRowLastCell
                    );

                    if (!isNullOrUndefined(cellsToFocus) && cellsToFocus.length > 0) {
                        (cellsToFocus[0] as HTMLElement).focus({ preventScroll: true });
                    }
                }
            }
        }
        return gridInitializeResults;
    }

    public adjustHeightsForTextWrap(): void {
        const contentTable: HTMLTableElement | null = this.getContent().querySelector('.e-table');
        if (!contentTable) {
            return;
        }
        if (contentTable.querySelector('tr.e-emptyrow')) {
            return;
        }
        const cells: NodeListOf<HTMLTableCellElement> = contentTable.querySelectorAll('td');
        if (!cells.length) {
            return;
        }
        for (let i: number = 0; i < cells.length; i++) {
            // eslint-disable-next-line security/detect-object-injection
            const cell: HTMLTableCellElement = cells[i];
            const headerCellHeight: number = parseFloat(window.getComputedStyle(cell, '::before').getPropertyValue('height')) || 0;
            const actualCellHeight: number = cell.offsetHeight;
            if (headerCellHeight > actualCellHeight) {
                cell.style.height = `${headerCellHeight}px`;
                cell.style.boxSizing = 'content-box';
            }
        }
    }

    public lastRowBorderCheck(): void {
        const contentETable: Element = this.getContent().querySelector('.e-table');
        if (!this.options.enableVirtualization && !isNullOrUndefined(contentETable) && !isNullOrUndefined(this.dotNetRef)) {
            if (contentETable.scrollHeight < this.getContent().clientHeight) {
                this.dotNetRef.invokeMethodAsync('LastRowBorder', true);
            }
            else if (this.options.editMode === 'Batch' && this.options.height !== 'auto' && this.options.height !== '100%' ) {
                let maxRows: number = Math.floor(this.getContent().clientHeight / this.getRowHeight());
                if (this.options.frozenRows > 0) {
                    maxRows += this.options.frozenRows;
                }
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                (this.dotNetRef.invokeMethodAsync('MaximumVisibleRows', maxRows) as any).catch(()=> {});
            }
        }
    }

    public addTableBorderClass(): void {
        const headerTable: Element = this.getHeaderTable();
        const contentTable: Element = this.getContentTable();
        let footerTable: Element;
        if (!isNullOrUndefined(this.getFooterContent())) {
            footerTable = this.getFooterContent().querySelector('.e-table');
        }
        const tableWidth: number = (headerTable as HTMLElement).offsetWidth;
        const contentwidth: number = (this.getContent().scrollWidth);
        if (contentwidth > tableWidth) {
            headerTable.classList.add('e-tableborder');
            contentTable.classList.add('e-tableborder');
            if (!isNullOrUndefined(footerTable)) {
                footerTable.classList.add('e-tableborder');
            }
        } else {
            headerTable.classList.remove('e-tableborder');
            contentTable.classList.remove('e-tableborder');
            if (!isNullOrUndefined(footerTable)) {
                footerTable.classList.remove('e-tableborder');
            }
        }
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    public wireEvents(): any {
        EventHandler.add(this.element, 'mousedown', this.mouseDownHandler, this);
        EventHandler.add(this.element, 'customContextMenuEvent', this.contextMenuHandler, this);
        EventHandler.add(this.element, 'focus', this.gridFocus, this);
        this.delegateClickHandler = this.documentClickHandler.bind(this);
        EventHandler.add(document, 'click', this.delegateClickHandler, this);
        EventHandler.add(this.element, 'keydown', this.gridKeyDownHandler, this);
        EventHandler.add(this.element, 'keydown', this.keyDownHandler, this);
        this.delegateKeyDownHandler = this.documentKeyHandler.bind(this);
        EventHandler.add(document.body, 'keydown', this.delegateKeyDownHandler, this);
        EventHandler.add(this.getContent(), 'touchstart', this.tapEvent, this);
        EventHandler.add(this.getContent(), 'click', this.preventClickOnDrag , this);
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        EventHandler.add(window as any, 'resize', this.windowResized, this);
        if (this.options.allowEditing) {
            EventHandler.add(this.element, 'dblclick', this.doubleClickHandler, this);
        }
        if (this.options.enableColumnVirtualization && this.options.allowPaging) {
            // Custom event to handle shift+tab navigation in column virtualization with paging enabled when focus is on the pager
            // Bind a method to your custom event 'shiftTabNavigation'
            EventHandler.add(this.element, 'shiftTabNavigation', this.handleShiftTabNavigation, this);
        }
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    public unWireEvents(): any {
        EventHandler.remove(this.element, 'mousedown', this.mouseDownHandler);
        EventHandler.remove(this.element, 'customContextMenuEvent', this.contextMenuHandler);
        EventHandler.remove(this.element, 'focus', this.gridFocus);
        EventHandler.remove(document, 'click', this.delegateClickHandler);
        EventHandler.remove(this.element, 'keydown', this.gridKeyDownHandler);
        EventHandler.remove(this.element, 'keydown', this.keyDownHandler);
        EventHandler.remove(document.body, 'keydown', this.delegateKeyDownHandler);
        EventHandler.remove(this.element, 'dblclick', this.doubleClickHandler);
        EventHandler.remove(this.getContent(), 'touchstart', this.tapEvent);
        EventHandler.remove(this.getContent(), 'click', this.preventClickOnDrag);
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        EventHandler.remove(window as any, 'resize', this.windowResized);
        if (this.options.enableColumnVirtualization && this.options.allowPaging) {
            EventHandler.remove(this.element, 'shiftTabNavigation', this.handleShiftTabNavigation);
        }
    }

    private windowResized(): void {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const _this: SfGrid = this;
        setTimeout(function (): void {
            const content: HTMLElement = <HTMLElement>_this.element.querySelector('.e-content.e-yscroll');
            if (_this.options.frozenColumns && (_this.options.width === '100%' || 'auto')) {
                if (_this.options.allowTextWrap && !isNullOrUndefined(_this.freezeModule)) {
                    _this.freezeModule.refreshRowHeight();
                }
            }
            const gridHeader: HTMLElement = <HTMLElement>_this.element.querySelector('.e-gridheader');
            if (!isNullOrUndefined(content) && content.scrollHeight > content.clientHeight) {
                if (!isNullOrUndefined(gridHeader)) {
                    gridHeader.style.paddingRight = getScrollBarWidth() - _this.scrollModule.getThreshold() + 'px';
                }
            }

            else {
                _this.scrollModule.setPadding();
            }
            const gridContent: HTMLElement = <HTMLElement>_this.element.querySelector('.e-gridcontent');
            const hasGridContentHeight: boolean = !isNullOrUndefined(gridContent) && gridContent.style.height !== '';
            const startIndex: number = hasGridContentHeight ? gridContent.style.height.indexOf('- ') + 2 : 0;
            const endIndex: number = hasGridContentHeight ? gridContent.style.height.indexOf('p') : 0;
            const sibilingsHeight: number = hasGridContentHeight ? parseInt(gridContent.style.height.slice(startIndex, endIndex), 10) : 0;
            const height: number = hasGridContentHeight ? getSiblingsHeight(gridContent) : 0;
            if (height !== sibilingsHeight) {
                _this.scrollModule.refresh();
            }
            if (!Browser.isDevice)
            {
                const activeElement: HTMLElement = document.activeElement as HTMLElement;
                const isTextInput: boolean = !isNullOrUndefined(activeElement) && /^(INPUT|TEXTAREA)$/.test(activeElement.tagName);
                const isLandscapeInputFocused: boolean = _this.isLandScapeMobile() && isTextInput;
                if (!isNullOrUndefined(_this.element) && (_this.element.querySelector('.e-filter-popup.e-popup-open') || _this.element.querySelector('.e-ccdlg.e-popup-open'))) {
                    if (!isLandscapeInputFocused && !isNullOrUndefined(_this.dotNetRef)) {
                        _this.dotNetRef.invokeMethodAsync('FilterPopupClose');
                    }
                }
                // Close enhanced operator dropdowns on window resize
                _this.closeOperatorDropdownIfOpen();
            }
            if (!isNullOrUndefined(_this.columnChooserModule)) {
                _this.columnChooserModule.windowResized();
            }
            if (_this.isResetDataTriggered && (_this.options.width === '100%' || _this.options.width === 'auto') && _this.options.enableVirtualization && !_this.options.enableColumnVirtualization && _this.options.allowResizing) {
                const contentTable: HTMLElement = _this.getContentTable() as HTMLElement;
                const headerTable: HTMLElement = _this.getHeaderTable() as HTMLElement;
                if (!isNullOrUndefined(contentTable) && !isNullOrUndefined(headerTable) && headerTable.offsetWidth > 0 && contentTable.offsetWidth > 0) {
                    contentTable.style.width = headerTable.offsetWidth + 'px';
                }
            }

        }, 100);

    }

    /**
     * Closes enhanced operator dropdowns if any are open.
     * This method safely checks for the presence of open operator dropdowns
     * and triggers the close action via Blazor interop.
     *
     * @public
     * @returns {void}
     */
    public closeOperatorDropdownIfOpen(): void {
        if (!isNullOrUndefined(this.dotNetRef) && !isNullOrUndefined(document)) {
            const operatorDropdowns: NodeListOf<Element> = document.querySelectorAll('.e-enhanced-operator-dropdown.e-popup-open');
            if (!isNullOrUndefined(operatorDropdowns) && operatorDropdowns.length > 0) {
                this.dotNetRef.invokeMethodAsync('CloseEnhancedOperatorDropdown');
            }
        }
    }

    private doubleClickHandler(e: MouseEventArgs): void {
        if ((e.target as HTMLElement).tagName === 'TD') {
            (e.target as HTMLElement).blur();
        }
        if (!isNullOrUndefined(this.toolTipModule)) {
            this.toolTipModule.close();
        }
    }

    private tapEvent = (e: MouseEventArgs): void => {
        if (!isNullOrUndefined(this.resizeModule) && this.resizeModule.getUserAgent()) {
            if (!Global.timer) {
                Global.timer = setTimeout(function (): void {
                    Global.timer = null;
                }, 300);
            }
            else {
                clearTimeout(Global.timer);
                Global.timer = null;
                const clickEvent: MouseEvent = new MouseEvent('dblclick', {
                    bubbles: true,
                    cancelable: true
                });
                e.target.dispatchEvent(clickEvent);
            }
        }
    };

    public setOptions(newOptions: IGridOptions, options: IGridOptions): void {
        const oldOptions: IGridOptions = <IGridOptions>extend(options, {});
        this.options = newOptions;
        if (!oldOptions.allowResizing && newOptions.allowResizing && !isNullOrUndefined(this.resizeModule)) {
            this.resizeModule.render();
        }
        if ((!oldOptions.allowGrouping && newOptions.allowGrouping)
            || (!oldOptions.allowReordering && newOptions.allowReordering) || newOptions.showDropArea) {
            if (!isNullOrUndefined(this.headerDragDrop)) {
                this.headerDragDrop.initializeHeaderDrag();
                this.headerDragDrop.initializeHeaderDrop();
            }
            if (!isNullOrUndefined(this.groupModule)) {
                this.groupModule.initializeGHeaderDrag();
                this.groupModule.initializeGHeaderDrop();
            }
            if (newOptions.allowReordering && !isNullOrUndefined(this.reorderModule)) {
                this.reorderModule.createReorderElement();
            }
        }

        if ((oldOptions.clipMode !== newOptions.clipMode) || newOptions.isColumnClipModeChanged) {
            this.toolTipModule.updateEvents();
        }

        if (!oldOptions.allowGrouping && newOptions.allowGrouping) {
            this.contentDragDrop.initializeContentDrop();
        }

        if (!oldOptions.allowRowDragAndDrop && newOptions.allowRowDragAndDrop) {
            this.rowDragAndDropModule.initializeDrag();

            if (this.options.isRenderedFromTreeGrid) {
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                const treeGridObj: any = (this.element.parentElement as any).blazor_instance;
                treeGridObj.rowDragAndDropModule.updateRowDragEvents();
            }
        } else if (oldOptions.allowRowDragAndDrop && !newOptions.allowRowDragAndDrop) {
            this.rowDragAndDropModule.destroy();
        }

        if (!this.isRendered) {
            this.isRendered = this.options.isPrerendered;
        }

        if (oldOptions.groupCount !== newOptions.groupCount || (oldOptions.showGroupedColumn !== newOptions.showGroupedColumn)) {
            const cell: Element = this.getHeaderTable().querySelector('.e-emptycell');
            if (!cell) { return; }
            cell.classList.remove('e-indentRefreshed');
        }
    }
    public documentClickHandler(e: MouseEventArgs): void {
        const CCButton: Element = parentsUntil(<Element>e.target, 'e-cc-toolbar');
        const formElement: Element = parentsUntil(<Element>e.target, 'e-gridform');
        const toolbar: Element = parentsUntil(<Element>e.target, 'e-toolbar-item');
        const cellElement: Element = parentsUntil(<Element>e.target, 'e-rowcell');
        const isExcel: Element = document.querySelector('.e-dialog.e-excelfilter.e-popup-open');
        this.virtualContentModule.selectedCellNavigation = -1;
        const gridElementId: string = isNullOrUndefined(parentsUntil(<Element>e.target, 'e-grid')) ? null : parentsUntil(<Element>e.target, 'e-grid').id;
        const targetGridId: string | null = !isNullOrUndefined(parentsUntil(cellElement, 'e-grid')) ? parentsUntil(cellElement, 'e-grid').id : null;
        if (parentsUntil(<Element>e.target, 'e-inline-edit') && gridElementId === this.element.id) {
            const cell: Element = parentsUntil(<Element>e.target, 'e-rowcell');
            this.editedCellIndex = (cell as HTMLTableCellElement) == null ? null : (cell as HTMLTableCellElement).cellIndex;
        }

        else {
            if (isNullOrUndefined(parentsUntil(<Element>e.target, 'e-popup-open'))) {
                this.editedCellIndex = null;
            }
        }
        if (!this.options.enableAdaptiveUI && !this.targetIsFilterDialog(e) && !((<Element>e.target).classList.contains('e-cc-cancel')) && !((<Element>e.target).classList.contains('e-choosercheck')) && !((<Element>e.target).classList.contains('e-icon-filter')) && !CCButton && (this.element.querySelectorAll('.e-filter-popup.e-popup-open').length || this.element.querySelectorAll('.e-ccdlg.e-popup-open').length)) {
            if (this.element.querySelector('.e-datetimepicker') != null) {
                (this.element.querySelector('.e-datetimepicker') as HTMLElement).blur();
            }
            if (!isNullOrUndefined(isExcel)){
                setTimeout(() => {
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    (this.dotNetRef.invokeMethodAsync('FilterPopupClose') as any).catch(()=> {});
                }, 10);
            }
            else{
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                (this.dotNetRef.invokeMethodAsync('FilterPopupClose') as any).catch(()=> {});
            }
        }
        if ((this.options.editMode === 'Batch' || this.options.editMode === 'Cell') && isNullOrUndefined(formElement) && (!cellElement || (!isNullOrUndefined(targetGridId) && (this.element.id !== targetGridId))) && !toolbar && !this.targetIsFilterDialog(e) && this.element.querySelector('.e-gridform')) {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            (this.dotNetRef.invokeMethodAsync('UpdateChanges') as any).catch(()=> {});
        }
        if (!Browser.isDevice) {
            this.toolTipModule.close();
        }
        const checkboxfilter: Element = parentsUntil(<Element>e.target, 'e-checkboxfilter');
        if (!isNullOrUndefined(checkboxfilter) || !isNullOrUndefined(parentsUntil(<Element>e.target, 'e-excelfilter'))) {
            const filter: Element = checkboxfilter || parentsUntil(<Element>e.target, 'e-excelfilter');
            if (isNullOrUndefined(filter)) {
                return;
            }

            const selectAll: HTMLElement = filter.querySelector('.e-selectall') as HTMLElement;
            const selectAllWrap: Element = selectAll ? (parentsUntil(selectAll, 'e-ftrchk') || selectAll.closest('.e-ftrchk')) : null;

            // Detect if header (Select All) row was clicked
            const rowWithClick: Element = (e.target as Element).closest('.e-ftrchk') || (e.target as Element).closest('li');
            const isHeaderClick: boolean = !!(rowWithClick && selectAll && rowWithClick.contains(selectAll));

            if (isHeaderClick) {
                if (selectAllWrap && !selectAllWrap.classList.contains('e-chkfocus')) {
                    selectAllWrap.classList.add('e-chkfocus');
                }
                if (selectAll) {
                    selectAll.focus();
                }
            } else {
                if (selectAllWrap) {
                    selectAllWrap.classList.remove('e-chkfocus');
                }
                const item: Element = (e.target as Element).closest('li[uid]') || (e.target as Element).closest('[uid]');
                const valueCheckbox: HTMLElement = item ? item.querySelector('.e-chk-hidden') : null;
                if (valueCheckbox) {
                    valueCheckbox.focus();
                }
            }
        }
    }
    public targetIsFilterDialog(e: MouseEventArgs): boolean {
        const activeElement: Element = document.activeElement;
        const popupElement: Element = parentsUntil(parentsUntil(<Element>e.target, 'e-filter-popup'), 'e-popup-open');
        const filterDropdown: Element = parentsUntil(parentsUntil(<Element>e.target, 'e-ddl'), 'e-popup-open');
        const activeFilterDropdown: Element = filterDropdown ? filterDropdown : parentsUntil(parentsUntil(activeElement, 'e-ddl'), 'e-popup-open');
        const filterCheckbox: Element = parentsUntil(parentsUntil(<Element>e.target, 'e-selectall'), 'e-ftrchk');
        const searchClear: Element = parentsUntil(<Element>e.target, 'e-chkcancel-icon');
        const isBlankCheckbox: boolean = ((<Element>e.target).classList.contains('e-check') || (<Element>e.target).classList.contains('e-uncheck')) && !!document.elementFromPoint(e.clientX, e.clientY)?.closest('.e-popup-open');
        const ccPopupElement: Element = parentsUntil(parentsUntil(<Element>e.target, 'e-ccdlg'), 'e-popup-open');
        const mentionElement: Element = parentsUntil(parentsUntil(<Element>e.target, 'e-ccdlg-mention'), 'e-popup-open');
        const dropDownTreeElement: Element = parentsUntil(parentsUntil(<Element>e.target, 'e-ddt'), 'e-popup-open');
        const zoomInElement: Element = parentsUntil(<Element>e.target, 'e-zoomin');
        const isScrollbar: boolean = e.target != null && (<HTMLElement>e.target).clientWidth > 0 && (<HTMLElement>e.target).clientHeight > 0 && (e.offsetX > (<HTMLElement>e.target).clientWidth || e.offsetY > (<HTMLElement>e.target).clientHeight);
        if (isScrollbar) {
            return false;
        }
        if (popupElement || filterDropdown || filterCheckbox || searchClear || this.isOutsideGridComponent(e) || zoomInElement ||
            isBlankCheckbox || ccPopupElement || mentionElement || dropDownTreeElement || activeFilterDropdown) {
            return true;
        }
        else {
            return false;
        }
    }
    private isOutsideGridComponent(e: MouseEventArgs): boolean {
        const elements: { className: string, popupSuffix: string }[] = [
            { className: 'e-datepicker', popupSuffix: '_popup' },
            { className: 'e-datetimepicker', popupSuffix: '_popup' },
            { className: 'e-timepicker', popupSuffix: '_popup' },
            { className: 'e-daterangepicker', popupSuffix: '_popup' }
        ];
        const isPickerInGrid: boolean = elements.some(({ className, popupSuffix }: { className: string; popupSuffix: string }) => {
            let pickerElement: Element | null = <Element>this.element.querySelector(`.${className}`);
            const pickerInEvent: Element | null = parentsUntil(<Element>e.target, className);
            const pickerElements: NodeListOf<Element> = this.element.querySelectorAll(`.${className}`);
            if (!isNullOrUndefined(pickerInEvent) && pickerElements.length > 0) {
                pickerElement = Array.from(pickerElements)
                    .find((el: Element) => '' + el.id + popupSuffix === pickerInEvent.id);
            }
            const isExpanded: string | null = !isNullOrUndefined(pickerInEvent) && !isNullOrUndefined(pickerElement)
                && `${pickerElement.id}${popupSuffix}` === pickerInEvent.id ? (this.options.isWebAssembly ? 'true' : pickerElement.getAttribute('aria-expanded')) : 'false';
            return isExpanded === 'true';
        });
        return isPickerInGrid;
    }
    private documentKeyHandler(e: KeyboardEventArgs): void {
        if (e.altKey && e.keyCode === 74 && !isNullOrUndefined(this.element)) {
            const grids: NodeListOf<Element>  = document.querySelectorAll('.e-grid');
            const targetGrid: Element = parentsUntil(<Element>e.target, 'e-grid');
            if (grids.length > 1) {
                if (isNullOrUndefined(targetGrid)) {
                    (grids[0] as HTMLElement).focus();
                    return;
                }
                (targetGrid as HTMLElement).focus();
            } else {
                this.element.focus();
            }
            this.dotNetRef.invokeMethodAsync('GridFocus', e);
        }
        if (e.altKey && e.keyCode === 87 && !isNullOrUndefined(this.element)) {
            const isPagerFocused: boolean = !isNullOrUndefined(parentsUntil(<Element>e.target, 'e-pager'));
            const isSearchInput: boolean = false;
            const targetGrid: Element = parentsUntil(<Element>e.target, 'e-grid');
            if (!isNullOrUndefined(targetGrid)) {
                return; // returned here since this action will get performed on Grid KeyDown handler.
            }
            this.dotNetRef.invokeMethodAsync('GridKeyDown', {
                key: e.key,
                code: e.code,
                ctrlKey: e.ctrlKey,
                shiftKey: e.shiftKey,
                altKey: e.altKey
            }, isSearchInput, isPagerFocused, false, this.editedCellIndex, null, null, false, false);
        }
    }

    public iterateTemplateElementsForward(columnTemplateElements: HTMLCollection): Element {
        for (let i: number = 0; i < columnTemplateElements.length; i++) {
            const currentElement: Element = columnTemplateElements[parseInt(i.toString(), 10)];
            if ((currentElement as HTMLElement).tabIndex === 0) {
                this.firstFocusableTemplateElement = currentElement;
                break;
            } else if (!isNullOrUndefined(currentElement.children) && currentElement.children.length !== 0) {
                this.iterateTemplateElementsForward(currentElement.children);
                break;
            }
        }
        return this.firstFocusableTemplateElement;
    }

    public iterateTemplateElementsBackward(columnTemplateElements: HTMLCollection): Element {
        for (let i: number = columnTemplateElements.length - 1; i >= 0; i--) {
            const currentElement: Element = columnTemplateElements[parseInt(i.toString(), 10)];
            if ((currentElement as HTMLElement).tabIndex === 0) {
                this.lastFocusableTemplateElement = currentElement;
                break;
            } else if (!isNullOrUndefined(currentElement.children) && currentElement.children.length !== 0) {
                this.iterateTemplateElementsBackward(currentElement.children);
                break;
            }
        }
        return this.lastFocusableTemplateElement;
    }

    public keyDownHandler(e: KeyboardEventArgs): void {
        const gridElement: Element = parentsUntil(<Element>e.target, 'e-grid');
        const elementTag: string = (e.target as HTMLElement).tagName;
        const isPagerFocused: boolean = !isNullOrUndefined(parentsUntil(<Element>e.target, 'e-pager'));
        const isGridToolbarFocused: boolean = !isNullOrUndefined(parentsUntil(<Element>e.target, 'e-toolbar-items'));
        let isSearchInput: boolean = false;
        let focusTemplateCell: boolean = false;
        let cellIndex: number;
        let rowIndex: number;

        
        //Keep the custom grid navigation in charge of Tab/Shift+Tab at the first/last grid cell.
       // Otherwise the browser restores focus to the earliest grid cell and breaks the previous-cell traversal.
        if (gridElement && e.key === 'Tab' && e.shiftKey) {
            const focusedCell: Element | null = parentsUntil(<Element>e.target, 'e-rowcell') || parentsUntil(<Element>e.target, 'e-headercell');
            if (!isNullOrUndefined(focusedCell) && isNullOrUndefined(parentsUntil(<Element>e.target, 'e-editing'))) {
                const row: Element | null = parentsUntil(focusedCell, 'e-row');
                if (!isNullOrUndefined(row)) {
                    const cells: HTMLElement[] = Array.from(row.querySelectorAll('.e-rowcell:not(.e-hide), .e-headercell:not(.e-hide)')) as HTMLElement[];
                    if (cells.length > 0) {
                        const lastCell: HTMLElement = cells[cells.length - 1];
                        if (focusedCell === lastCell) {
                            e.preventDefault();
                        }
                    }
                }
            }
        }
        //when shift+tab pressed from first content cell, focus get stuck in filterBar cell
        if (gridElement) {
            const headerCells: HTMLElement[] = Array.from(gridElement.querySelectorAll('.e-headercell')).filter((cell: HTMLElement) => cell.offsetParent !== null) as HTMLElement[];
            const filterBarCell: Element = parentsUntil(document.activeElement, 'e-filterbarcell');
            if ((e.shiftKey && e.key === 'Tab') && filterBarCell) {
                let previousFilterBarCell: HTMLElement | null = filterBarCell.previousElementSibling as HTMLElement;
                const currentCell = filterBarCell as HTMLElement;
                const target = e.target as HTMLElement;

                const hasEnhancement: boolean = currentCell.querySelector('.e-enhancement-filterbar') !== null;
                const isFilterIcon: boolean = target?.classList.contains('e-icons') &&
                    (target.classList.contains('e-filter') || target.classList.contains('e-filter-clear'));

                // Handle first filter bar edge case
                if (hasEnhancement && !previousFilterBarCell && isFilterIcon) {
                    previousFilterBarCell = currentCell;
                }

                if (previousFilterBarCell) {
                    const isEnhancedPrevCell: boolean = previousFilterBarCell.querySelector('.e-enhancement-filterbar') !== null;

                    if (!isEnhancedPrevCell) {
                        const filterBarInput: HTMLInputElement | null = 
                            (previousFilterBarCell.querySelector('[tabindex="0"]') as HTMLInputElement | null) ??
                            (previousFilterBarCell.querySelector('input') as HTMLInputElement | null);

                        if (filterBarInput) {
                            filterBarInput.focus();
                            e.preventDefault();
                        }
                    }
                } else if (!isNullOrUndefined(headerCells)) {
                    headerCells[headerCells.length - 1].focus();
                    e.preventDefault();
                }
            }

            if (e.key === 'Tab' && !e.shiftKey) {
                const filterBarRow: Element = parentsUntil(document.activeElement, 'e-filterbar');
                if (filterBarRow && (filterBarRow.lastElementChild === filterBarCell)) {
                    const firstRow: HTMLTableRowElement | null = this.element.querySelectorAll('.e-row:not(.e-masked-row.e-hidden)')[0] as HTMLTableRowElement;
                    if (firstRow && firstRow.classList.contains('e-insertedrow')) {
                        const firstVisibleCell: HTMLTableCellElement | null = this.getFirstVisibleCell(firstRow) as HTMLTableCellElement;
                        if (firstVisibleCell) {
                            firstVisibleCell.focus();
                            e.preventDefault();
                        }
                    }
                }
                else if (filterBarCell && this.element.querySelectorAll('.e-insertedrow').length > 1) {
                    const nextFilterBarCell: HTMLElement | null = filterBarCell.nextElementSibling as HTMLElement | null;
                    if (nextFilterBarCell) {
                        const inputElement: HTMLInputElement | null = nextFilterBarCell.querySelector<HTMLInputElement>('input');
                        if (inputElement) {
                            inputElement.focus();
                        }
                    }
                }
            }
        }
        if ((gridElement && gridElement.id !== this.element.id) ||
            (e.key === 'Shift' || e.key === 'Control' || e.key === 'Meta' || e.key === 'Alt')) {
            return;
        }

        if ((elementTag === 'INPUT' || elementTag === 'TEXTAREA') && e.code === 'Delete') {
            return;
        }

        if (!isNullOrUndefined(gridElement) && !isNullOrUndefined(gridElement.querySelector('.e-templatecell')) && !isNullOrUndefined(parentsUntil(<Element>e.target, 'e-templatecell'))) {
            const templateCell: Element = parentsUntil(<Element>e.target, 'e-rowcell') || (this.options.isRenderedFromTreeGrid && !isNullOrUndefined(e.code) && e.code === 'Escape' && parentsUntil(<Element>e.target, 'e-templatecell')) ;
            if (isNullOrUndefined(templateCell) || !(templateCell.firstElementChild)) { return; }
            cellIndex = (templateCell as HTMLTableCellElement).cellIndex;
            rowIndex = (Number)(parentsUntil(<Element>e.target, 'e-row').getAttribute('aria-rowIndex')) - 1;
            const templateElements: HTMLCollection = templateCell.firstElementChild.children;
            const firstFocussableElement: Element = this.iterateTemplateElementsForward(templateElements);
            const lastFocussableElement: Element = this.iterateTemplateElementsBackward(templateElements);
            const isTabKey: boolean = !e.shiftKey && e.code === 'Tab';
            const isShiftTabKey: boolean = e.shiftKey && e.code === 'Tab';
            const isEscapeKey: boolean = e.code === 'Escape';

            if ((e.target === firstFocussableElement && isShiftTabKey) || (e.target === lastFocussableElement && isTabKey) || isEscapeKey ||
                (firstFocussableElement === null && lastFocussableElement === null && !((e.target as HTMLElement).classList.contains('e-templatecell')) && (isTabKey || isShiftTabKey))) {
                focusTemplateCell = true;
            }
        }

        if ((e.target as HTMLElement).classList.contains('e-searchinput') && e.key === 'Enter') {
            isSearchInput = true;
        }

        const allow: boolean = this.isPopUpOpened(e);
        if (allow && (e.key === 'Escape' || e.key === 'Enter') && document.activeElement.tagName === 'BODY') {
            (e.target as HTMLElement).focus();
        }
        // Handle keyboard events for popup components in batch/cell editing mode
        let isPopupActive: boolean = false;
        const isBatchEditpopupRow: boolean = !isNullOrUndefined(parentsUntil((e.target as Element), 'e-editedbatchcell')) && allow;
        if(isBatchEditpopupRow)
        {
            // Check if the popup is currently expanded
            isPopupActive = (e.target as Element).getAttribute('aria-expanded') === 'true';
            if((e.target as Element).classList.contains('e-dropdownlist')) {
                isPopupActive = elementTag === 'INPUT' && (e.target as Element).closest('.e-ddl')?.getAttribute('aria-expanded') === 'true';
            }
        }
        // Handle Escape key to close popups in batch/cell editing
        if (e.key === 'Escape' && allow && ((e.target as Element).getAttribute('aria-expanded') === 'true')) {
            return;
        }
        if (!isNullOrUndefined(parentsUntil(<Element>e.target, 'e-rowcell'))) {
            this.editedCellIndex = (parentsUntil(<Element>e.target, 'e-rowcell') as HTMLTableCellElement).cellIndex === this.editedCellIndex ? this.editedCellIndex : null;
        }
        if (!isNullOrUndefined(parentsUntil((e.target as Element), 'e-ccdlg')) && (e.key === 'Tab' || e.key === 'ArrowDown' || e.key === 'ArrowUp')) {
            this.focusColumnChooserDialogElements(e);
            return;
        }
        //filter
        if ((e.key === 'Tab' || e.key === 'ArrowDown' || e.key === 'ArrowUp') &&
            (!isNullOrUndefined(parentsUntil((e.target as Element), 'e-excelfilter'))
                || !isNullOrUndefined(parentsUntil((e.target as Element), 'e-checkboxfilter')))) {
            this.focusFilterDialogElements(e);
            return;
        }
        const isDialogOpen: boolean = !!this.element.querySelector('.e-popup-open');
        const ccDlgOpen: boolean = isDialogOpen && !!this.element.querySelector('.e-ccdlg');
        const editedRowNavigation: boolean = !!parentsUntil((e.target as Element), 'e-editedrow')
            && (e.key === 'Tab' || (e.key === 'Tab' && e.shiftKey));
        if (focusTemplateCell || e.key === 'F2' || e.key === 'Insert' || e.key === 'Delete' || (e.key === 'p' && e.ctrlKey)
            || ((!isDialogOpen || ccDlgOpen) && e.key === 'Escape') || editedRowNavigation
            || isPopupActive || (e.altKey && e.keyCode === 87) ||
            (e.action === 'pageUp' || e.action === 'pageDown' || e.action === 'ctrlAltPageUp'
                || e.action === 'ctrlAltPageDown' || e.action === 'altPageUp' || e.action === 'altPageDown')) {
            this.dotNetRef.invokeMethodAsync('GridKeyDown', {
                key: e.key,
                code: e.code,
                ctrlKey: e.ctrlKey,
                shiftKey: e.shiftKey,
                altKey: e.altKey,
                metaKey: e.metaKey
            }, isSearchInput, isPagerFocused, isGridToolbarFocused,
                                             this.editedCellIndex, rowIndex, cellIndex, focusTemplateCell, isPopupActive);
        }
    }

    private focusColumnChooserDialogElements(e: KeyboardEventArgs): void {
        const dialog: Element = parentsUntil((e.target as Element), 'e-dialog');
        const items: Element[] = Array.from(dialog.querySelectorAll('.e-chk-hidden, .e-btn, .e-ccsearch'));
        if (!items.length) { return; }
        e.preventDefault();
        // clear previous focus
        const removeFocusClass: (selector: string) => void = function (selector: string): void {
            dialog.querySelectorAll(selector).forEach(function (el: Element): void {
                el.classList.remove('e-colfocus');
            });
        };
        const current: Element = document.activeElement;
        removeFocusClass('.e-cclist.e-colfocus');
        let tabCount: number = items.indexOf(current);
        if (tabCount === -1) {
            tabCount = (e.shiftKey || e.key === 'ArrowUp') ? items.length : -1;
        }
        const nextIdx: number = (e.shiftKey || e.key === 'ArrowUp')
            ? (tabCount <= 0 ? items.length - 1 : tabCount - 1)
            : (tabCount >= items.length - 1 ? 0 : tabCount + 1);
        const next: HTMLElement = items[parseInt(nextIdx.toString(), 10)] as HTMLElement;
        if (next) {
            next.focus();
            const focusElement: Element = parentsUntil(next, 'e-cclist') || parentsUntil(next, 'e-ccheck');
            if (focusElement) {
                focusElement.classList.add('e-colfocus');
            }
        }
    }

    private focusFilterDialogElements(e: KeyboardEventArgs): void {
        const gridElement: Element = parentsUntil((e.target as Element), 'e-grid');
        const dialog: Element = parentsUntil((e.target as Element), 'e-dialog');
        const items: Element[] = Array.from(dialog.querySelectorAll('.e-chk-hidden, .e-btn, .e-searchinput'));
        // Clear previous highlights
        const selElements: NodeListOf<Element> = gridElement.querySelectorAll('.e-ftrchk.e-chkfocus');
        selElements.forEach(function (el: Element): void {
            el.classList.remove('e-chkfocus');
        });
        const current: Element = document.activeElement;
        let tabCount: number = items.indexOf(current);
        if (tabCount === -1) {
            tabCount = (e.shiftKey || e.key === 'ArrowUp') ? items.length : -1;
        }
        const nextIdx: number = e.shiftKey || e.key === 'ArrowUp'
            ? (tabCount <= 0 ? items.length - 1 : tabCount - 1)
            : (tabCount >= items.length - 1 ? 0 : tabCount + 1);
        const next: HTMLElement = items[parseInt(nextIdx.toString(), 10)] as HTMLElement;
        if (next) {
            e.preventDefault(); // prevent default tab behavior
            next.focus();
            // Add e-colfocus to the parent .e-ftrchk if applicable
            const focusElement: Element = parentsUntil(next, 'e-ftrchk');
            if (focusElement) {
                focusElement.classList.add('e-chkfocus');
            }
        }
    }

    private getFirstVisibleCell(row: HTMLTableRowElement): HTMLTableCellElement | undefined {

        for (let i: number = 0; i < row.cells.length; i++) {
            const cell: HTMLTableCellElement = row.cells[parseInt(i.toString(), 10)];
            const isEmptyIndentCell: boolean = cell.classList.contains('e-indentcell') && cell.classList.contains('e-updatedtd');
            const firstVisiblecell: boolean = !isEmptyIndentCell && !cell.classList.contains('e-hide');
            if (firstVisiblecell) {
                return cell;
            }
        }

        return undefined;
    }

    private isLastCell(e: KeyboardEventArgs): boolean {
        const normalEditDiv: HTMLElement = (e.target as Element).closest('.e-normaledit') as HTMLElement;
        if (!normalEditDiv) {
            return false;
        }
        else {
            const visibleTds: Element[] = (Array.from(normalEditDiv.querySelectorAll('.e-rowcell'))).filter((e: Element) => !(e.classList.contains('e-hide') || e.querySelector('.e-disabled')));
            return visibleTds[visibleTds.length - 1] === parentsUntil(e.target as Element, 'e-rowcell');
        }
    }

    private isPopUpOpened(e: KeyboardEventArgs): boolean {
        const datePicker: boolean = (e.target as Element).classList.contains('e-datepicker');
        const dateTimePicker: boolean = (e.target as Element).classList.contains('e-datetimepicker');
        const timePicker: boolean = (e.target as Element).classList.contains('e-timepicker');
        const daterangePicker: boolean = (e.target as Element).classList.contains('e-daterangepicker');
        const multiSelect: boolean = (e.target as Element).classList.contains('e-multiselect');
        const dropDownList: boolean = !isNullOrUndefined(parentsUntil(<Element>e.target, 'e-ddl'));
        const autoComplete: boolean = (e.target as Element).classList.contains('e-autocomplete');
        const comboBox: boolean = (e.target as Element).classList.contains('e-combobox');
        return (datePicker || dateTimePicker || timePicker || daterangePicker || multiSelect || dropDownList || autoComplete || comboBox);
    }

    // custom event used for column virtualization with paging when shift + Tab is given from pager.
    private handleShiftTabNavigation(e: CustomEvent): void {
        const pagerTarget: HTMLElement = e.detail.currentTarget as HTMLElement;
        setTimeout((): void => {
            if (Math.round(this.getContent().scrollLeft + this.getContent().clientWidth) < this.getContent().scrollWidth &&
                !isNullOrUndefined(pagerTarget) && pagerTarget.classList.contains('e-pager')
                && document.activeElement.parentElement.tagName.toLocaleLowerCase() === 'tr') {
                this.getContent().scrollLeft = this.getContent().scrollWidth;
                this.virtualContentModule.focusFromPager = true;
            }
            else {
                this.virtualContentModule.focusFromPager = false;
            }
        }, 30);
    }

    public gridKeyDownHandler(e: KeyboardEventArgs): void {
        const isMacLike: boolean = navigator.userAgent.indexOf('Mac') !== -1;
        
        if (this.options.enableUndoRedo && this.options.editMode === 'Batch' &&
            (e.ctrlKey || (isMacLike && e.metaKey)) && (e.keyCode === 90 || e.keyCode === 89 || (e.shiftKey && e.keyCode === 90))) {
            const targetGrid: Element = parentsUntil(<Element>e.target, 'e-grid');
            if (!isNullOrUndefined(targetGrid) && targetGrid.id === this.element.id) {
                e.preventDefault(); // Prevent browser's default undo/redo
                this.dotNetRef.invokeMethodAsync('GridKeyDown', {
                    key: e.key,
                    code: e.code,
                    ctrlKey: e.ctrlKey,
                    shiftKey: e.shiftKey,
                    altKey: e.altKey,
                    metaKey: e.metaKey
                }, false, false, false, this.editedCellIndex, null, null, false, false);
                return;
            }
        }
        const headerCell: Element | null = (e.target as HTMLElement).closest('.e-headercell');
        if (e.shiftKey && e.key === 'Tab' && headerCell) {
            const findInput: HTMLInputElement = this.element.querySelector('.e-find-input:not([disabled])') as HTMLInputElement;
            const visibleHeaderCells: HTMLElement[] = Array.from(this.element.querySelectorAll('.e-headercell'))
                .filter((cell: HTMLElement) => cell.offsetParent !== null) as HTMLElement[];
            if (findInput && findInput.offsetParent !== null && visibleHeaderCells[0] === headerCell) {
                e.preventDefault();
                e.stopPropagation();
                findInput.focus();
                return;
            }
        }
        const popupElement: Element = parentsUntil(<Element>e.target, 'e-filter-popup');
        const elementTag: string = (e.target as HTMLElement).tagName;
        this.toolTipModule.close();
        if (!isNullOrUndefined(popupElement) && popupElement.classList.contains('e-popup-open') && e.key !== 'Escape') {
            e.stopPropagation();
            if ((e.key === 'Tab' || e.key === 'shiftTab' || e.key === 'Enter' || e.key === 'shiftEnter') &&
                (elementTag === 'INPUT' || elementTag === 'TEXTAREA')) {
                const evt: Event = new Event('change', {
                    bubbles: false,
                    cancelable: true
                });
                e.target.dispatchEvent(evt);
            }
        }

        // handling the focus for the template used inside edit settings.
        let shouldReturn: boolean = false;
        if (this.options.hasTemplateInEditSettings && parentsUntil(<Element>e.target, 'e-normaledit')) {
            const editFormInputElements: NodeListOf<HTMLInputElement> = this.element.querySelector('.e-normaledit').querySelectorAll('input');
            const editFormTextareaElements: NodeListOf<HTMLTextAreaElement> = this.element.querySelector('.e-normaledit').querySelectorAll('textarea');
            const editForm: (HTMLInputElement | HTMLTextAreaElement)[] = [...Array.from(editFormInputElements),
                ...Array.from(editFormTextareaElements)];
            shouldReturn = true;
            if (!isNullOrUndefined(editForm)) {
                const firstInput: HTMLInputElement | HTMLTextAreaElement = editForm[0];
                const lastInput: HTMLInputElement | HTMLTextAreaElement = editForm[editForm.length - 1];
                const isShiftTabKey: boolean = e.shiftKey && e.key === 'Tab';
                const isTabKey: boolean = !e.shiftKey && e.key === 'Tab';
                if ((document.activeElement === firstInput && isShiftTabKey) || (document.activeElement === lastInput && isTabKey)) {
                    shouldReturn = false;
                }
            }
        }

        // Handling the fix for the mouse click issue while using the EditTemplate in normal editing refer task: 876403
        if ((e as KeyboardEvent).key === 'Tab') {
            const normalEditDiv: HTMLElement = this.element.querySelector('.e-normaledit');
            if (!isNullOrUndefined(normalEditDiv)) {
                const targetElement: HTMLElement = e.target as HTMLElement;
                const isAutoCompleteOrMultiSelectOrTimePicker: boolean = targetElement.classList.contains('e-autocomplete') || targetElement.classList.contains('e-multiselect') || targetElement.classList.contains('e-timepicker');
                const visibleTds: Element[] = (Array.from(normalEditDiv.querySelectorAll('.e-rowcell'))).filter((e: Element) => !(e.classList.contains('e-hide') || e.querySelector('.e-disabled')));
                if (isAutoCompleteOrMultiSelectOrTimePicker && ((e.key === 'Tab' && !e.shiftKey && visibleTds[visibleTds.length - 1] !== parentsUntil(targetElement, 'e-rowcell')) || (e.key === 'Tab' && e.shiftKey && visibleTds[0] !== parentsUntil(targetElement, 'e-rowcell')))) {
                    return;
                }
            }
        }

        //TODO: datepicker in dialog editing
        //NOTE: The below if condition is added for this task BLAZ-7200, while ensuring this task suspecting the below condition is redundant need to ensure this.
        if (((e.key === 'Tab' || e.key === 'Escape' || e.key === 'shiftTab' || e.key === 'Enter' || e.key === 'shiftEnter')
            && (elementTag === 'INPUT' || elementTag === 'TEXTAREA' || (e.target as HTMLElement).classList.contains('e-datepicker') || (e.target as HTMLElement).classList.contains('e-datetimepicker'))) || ((e.target as HTMLElement).classList.contains('e-rowcell') && e.key === 'F2')) {
            const targetElement: HTMLElement = e.target as HTMLElement;
            // Prevent blur and allow Enter key to work for multiline input
            if (!(e.key === 'Enter' && elementTag === 'TEXTAREA' && this.options.editMode === 'Normal')) {
                if (!((e.key === 'Tab' && (targetElement.classList.contains('e-datepicker') || targetElement.classList.contains('e-datetimepicker'))) ||
                    (e.key === 'Enter' && targetElement.classList.contains('e-autocomplete') || targetElement.classList.contains('e-multiselect')))) {
                    if (e.key === 'Escape' || (e.key === 'Tab' && this.isLastCell(e))) {
                        targetElement.blur();
                    } else {
                        // Firefox needs additional delay for Tab navigation to work correctly
                        const delay: number = ((e.key === 'Tab' || e.key === 'shiftTab') && Browser.info.name === 'mozilla') ? 10 : 0;
                        setTimeout(() => {
                            targetElement.blur();
                        }, delay);
                    }
                }
            }
        }

        if (e.key === 'Shift' || e.key === 'Control' || e.key === 'Alt') {
            e.stopPropagation(); //dont let execute c# keydown handler for meta keys.
        }

        if (e.keyCode === 67 && (e.ctrlKey || (isMacLike && e.metaKey)) && !this.options.isRenderedFromTreeGrid) {
            this.clipboardModule.copy();
        } else if (e.keyCode === 72 && (e.ctrlKey || (isMacLike && e.metaKey)) && e.shiftKey && !this.options.isRenderedFromTreeGrid) {
            this.clipboardModule.copy(true);
        }
        if (e.keyCode === 86 && (e.ctrlKey || (isMacLike && e.metaKey)) && !this.options.isEdit) {
            const rowElement: Element = parentsUntil(<Element>e.target, 'e-rowcell');
            if (!isNullOrUndefined(rowElement) && !rowElement.classList.contains('e-templatecell')) {
                e.stopPropagation();
            }
            this.clipboardModule.pasteHandler();
        }

        const normalEditDivs: NodeListOf<HTMLElement> = this.element.querySelectorAll('.e-normaledit');
        let normalEditDiv: HTMLElement;
        if (!isNullOrUndefined(normalEditDivs)) {
            normalEditDivs.forEach(function (element: HTMLElement): void {
                if ((element as HTMLElement).contains(e.target as HTMLElement)) {
                    normalEditDiv = element;
                }
            });
        }
        if (parentsUntil(<Element>e.target, 'e-showAddNewRow') && e.shiftKey) {
            return;
        }
        //Preventing Duplicate records inserted in ShowAddNewRow when pressing tab continously
        if (e.key === 'Tab' && this.options.showAddNewRow) {
            const addNewRow: HTMLElement | null = this.getContent().querySelector('.e-showAddNewRow') as HTMLElement;
            if (!addNewRow || this.previousTarget === e.target) {
                this.previousTarget = null;
                return;
            }
            const targetRow: HTMLElement | null = e.target ? parentsUntil(e.target as HTMLElement, 'e-row') as HTMLElement | null : null;
            if (!targetRow) {
                return;
            }
            if (addNewRow === targetRow && this.isLastCell(e)) {
                this.previousTarget = e.target as HTMLElement | null;
            }
        }
        if (!isNullOrUndefined(normalEditDiv) && e.key === 'Tab') {
            const visibleTds: Element[] = (Array.from(normalEditDiv.querySelectorAll('.e-rowcell'))).filter((e: Element) => !(e.classList.contains('e-hide') || e.querySelector('.e-disabled') || e.querySelector('.e-checkbox-disabled')));

            if (shouldReturn) {
                return;
            }

            const commandColumnDiv: Element = parentsUntil(e.target as Element, 'e-unboundcelldiv');
            const targetCell: Element = parentsUntil(e.target as Element, 'e-rowcell');
            const isShiftPressed: boolean = e.shiftKey;
            const isCommandColumnNull: boolean = isNullOrUndefined(commandColumnDiv);
            const isLastCell: boolean = visibleTds[visibleTds.length - 1] === targetCell;
            const isFirstCell: boolean = visibleTds[0] === targetCell;
            const commandColumnChildCount: number = commandColumnDiv ? commandColumnDiv.childElementCount : 0;
            const isCommandColumnCheck: (index: number) => boolean = (index: number): boolean => {
                if (!isCommandColumnNull && (typeof index !== 'number' || index < 0 || index >= commandColumnDiv.children.length)) {
                    return false;
                }
                return isCommandColumnNull || (!isCommandColumnNull && commandColumnDiv.children.item(index) === e.target);
            };
            if ((!isShiftPressed && isLastCell && isCommandColumnCheck(commandColumnChildCount - 1))
                || (isShiftPressed && isFirstCell && isCommandColumnCheck(0))) {
                this.dotNetRef.invokeMethodAsync('EndEdit', {
                    key: e.key,
                    code: e.code,
                    ctrlKey: e.ctrlKey,
                    shiftKey: e.shiftKey,
                    altKey: e.altKey
                });
                e.preventDefault();
            }
        }

        if (this.element.querySelector('.e-batchrow')) {
            //new - for batch editing keys            
            const keys: string[] = ['a', 'control', 'Delete', 'Insert'];
            const actions: string[] = ['altPageUp', 'altPageDown', 'ctrlAltPageDown', 'ctrlAltPageUp', 'ctrlPlusP'];
            const keyboardNavKeys: string[] = ['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];

            if (!isNullOrUndefined(e.key) &&
                (keys.indexOf(e.key === 'A' ? 'a' : e.key) !== -1) || (e.key === 'F2' && !e.shiftKey) || actions.indexOf(e.action) !== -1
                || ((e.shiftKey || e.ctrlKey) && keyboardNavKeys.indexOf(e.key) !== -1)) {
                e.stopPropagation();
            }
            if (e.key === 'Tab' || e.key === 'shiftTab' || e.key === 'Enter' || e.key === 'shiftEnter') {
                e.preventDefault();
                if (elementTag === 'INPUT' || elementTag === 'TEXTAREA') {
                    const evt: Event = new Event('change', {
                        bubbles: false,
                        cancelable: true
                    });
                    e.target.dispatchEvent(evt);
                }
            }

        }

        if (this.options.selectionMode === 'Cell' && this.options.editMode === 'Batch' && parentsUntil(e.target as Element, 'e-gridform')) {
            const rows: HTMLElement = parentsUntil(parentsUntil(e.target as Element, 'e-gridform'), 'e-row') as HTMLElement;
            const cells: NodeListOf<HTMLTableCellElement> = rows.querySelectorAll('.e-rowcell:not(.e-hide)') as NodeListOf<HTMLTableCellElement>;
            const currentCell: HTMLTableCellElement = parentsUntil(e.target as Element, 'e-rowcell') as HTMLTableCellElement;
            if (!e.shiftKey && e.key === 'Tab') {
                for (let i: number = 0; i < cells.length; i++) {
                    const cell: HTMLTableCellElement = cells[parseInt(i.toString(), 10)] as HTMLTableCellElement;
                    if (cell.cellIndex === currentCell.cellIndex) {
                        if (i < cells.length - 1) {
                            cells[i + 1].tabIndex = 0;
                        }
                        break; // This will exit the for loop once the condition is satisfied
                    }
                }
            }
            else if (e.shiftKey && e.key === 'Tab') {
                for (let i: number = cells.length - 1; i >= 0; i--) {
                    const cell: HTMLTableCellElement = cells[parseInt(i.toString(), 10)];
                    if (cell.cellIndex === currentCell.cellIndex) {
                        if (i > 0) {
                            cells[i - 1].tabIndex = 0;
                        }
                        break; // This will exit the for loop once the condition is satisfied
                    }
                }
            }
        }

        if (this.options.enableColumnVirtualization) {
            this.virtualContentModule.columnVirtualizationKeyDownHandler(e);
        }
    }

    private contextMenuHandler(e: CustomEvent): void {
        const mouseEvent: MouseEventArgs = (e as CustomEvent).detail.contextmenu as MouseEventArgs;
        if (isNullOrUndefined(mouseEvent)) {
            return;
        }
        const gridElement: Element = parentsUntil(<Element>mouseEvent.target, 'e-grid');
        if (gridElement && gridElement.id !== this.element.id) {
            return;
        }
         this.handleContextMenuAction(mouseEvent, gridElement);
    }

    public mouseDownHandler(e: MouseEventArgs): void {
        this.preventSaveCellOnDragRelease = false;
        this._mouseDownX = e.clientX;
        this._mouseDownY = e.clientY;
        const gridElement: Element = parentsUntil(<Element>e.target, 'e-grid');
        if (this.options.enableVirtualization || this.options.enableColumnVirtualization) {
            this.virtualContentModule.observer.isWheelScrolling = false;
            this.virtualContentModule.observer.isTouchScrolling = false;
        }
        if (gridElement && gridElement.id !== this.element.id) {
            return;
        }
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const editFormElement: any = this.options.editMode === 'Normal' ? 'e-editedrow' : this.options.editMode === 'Batch' ? 'e-editedbatchcell' : null;
        if (!isNullOrUndefined(editFormElement) && editFormElement && parentsUntil(<Element>e.target, editFormElement)) {
            this.preventSaveCellOnDragRelease = true;
        }
        if (!this.options.enableAdaptiveUI && !this.targetIsFilterDialog(e) && (<Element>e.target).classList.contains('e-content') && (this.element.querySelectorAll('.e-filter-popup.e-popup-open').length || this.element.querySelectorAll('.e-ccdlg.e-popup-open').length)) {
            setTimeout(() => {
                this.dotNetRef.invokeMethodAsync('FilterPopupClose');
            }, 0);
        }
        this.closeOperatorDropdownIfOpen();
        const allowShiftSelection: (gridObj: SfGrid) => boolean = function (gridObj: SfGrid): boolean {
            const activeElement: Element = document.activeElement;
            return gridObj.element.querySelector('.e-editedrow, .e-editedbatchcell, .e-addedrow, .e-showAddNewRow') && activeElement && gridObj.element.contains(activeElement) && (activeElement.tagName === 'INPUT' || activeElement.tagName === 'TEXTAREA');
        };
        if ((e.shiftKey || e.ctrlKey) && !allowShiftSelection(this)) {
            e.preventDefault(); //prevent user select on shift pressing during selection
        }
        this.handleContextMenuAction(e, gridElement);
    }

    private handleContextMenuAction(e: MouseEventArgs, gridElement: Element): void {
        // e.button = 2 for right mouse button click
        // e.button = -1 for long press in touchscreen
        // This is used to handle cases where macOS users perform a "right-click" equivalent
        // by holding the Control key and clicking the left mouse button.
        const contextMenu: Element = document.querySelector('.e-sfcontextmenu');
        const isContextMenuElement: boolean = !isNullOrUndefined(contextMenu) ? contextMenu.classList.contains('e-grid-menu') : false;
        const isColumnMenuHide: boolean = !isNullOrUndefined(contextMenu) ? contextMenu.classList.contains('e-hide-menu') : false;
        const isShowColumnMenu: boolean = !isNullOrUndefined(contextMenu) ? contextMenu.classList.contains('e-grid-column-menu') : false;
        const isLeftClickWithMenu: boolean = isContextMenuElement && !isColumnMenuHide && !isShowColumnMenu && e.button === 0;
        const isMacControlLeftClick: boolean = e.button === 0 && e.ctrlKey && !e.metaKey && this.isMacOS() && isContextMenuElement;
        if (e.button !== -1 && e.button !== 2 && ((parentsUntil(<Element>e.target, 'e-headercell') && !isMacControlLeftClick) || parentsUntil(<Element>e.target, 'e-detailcell')) || parentsUntil(<Element>e.target, 'e-detailrowexpand') || parentsUntil(<Element>e.target, 'e-detailrowcollapse')
            || (<Element>e.target).classList.contains('e-headercontent') || closest(<Element>e.target, '.e-groupdroparea') || closest(<Element>e.target, '.e-gridpopup')
            || closest(<Element>e.target, '.e-summarycell') || closest(<Element>e.target, '.e-rhandler')
            || closest(<Element>e.target, '.e-filtermenudiv') || closest(<Element>e.target, '.e-filterbarcell')
            || closest(<Element>e.target, '.e-toolbar') || closest(<Element>e.target, '.e-toolbar-items')
            || closest(<Element>e.target, '.e-groupcaption')) {
            const defaultParams: (string | number | null)[] = [null, null, null, null];
            this.dotNetRef.invokeMethodAsync('MouseDownHandler', ...defaultParams);
        } else if (e.button === 2 || isLeftClickWithMenu || (e.type === 'contextmenu' && e.button === -1)
            || isMacControlLeftClick) {
            let target: string = null;
            let cellUid: string = null;
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            let rowUid: any = null;
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            let cellColIndex: any = null;
            const editForm: Element = parentsUntil(parentsUntil(<Element>e.target, 'e-gridform'), 'e-grid');
            if (parentsUntil(<Element>e.target, 'e-editcell') || editForm && editForm.id === gridElement.id) {
                target = 'Edit';
            } else if (parentsUntil(<Element>e.target, 'e-pager')) {
                target = 'Pager';
            } else if (parentsUntil(<Element>e.target, 'e-headercontent')) {
                target = 'Header';
                cellUid = parentsUntil(<Element>e.target, 'e-headercell') ? parentsUntil(<Element>e.target, 'e-headercell').getAttribute('data-uid') : null;
            } else if (parentsUntil(<Element>e.target, 'e-content')) {
                const targetElement = e.target as HTMLElement | null;
                const isScrollbarClick: boolean = targetElement != null && !(parentsUntil(targetElement, 'e-rowcell') || parentsUntil(targetElement, 'e-detailcell') || parentsUntil(targetElement, 'e-recordplusexpand') || parentsUntil(targetElement, 'e-recordpluscollapse')) && (e.offsetX > targetElement.clientWidth || e.offsetY > targetElement.clientHeight);
                if (isScrollbarClick) {
                    const defaultParams: (string | number | null)[] = [null, null, null, null];
                    this.dotNetRef.invokeMethodAsync('MouseDownHandler', ...defaultParams);
                    return;
                }
                target = 'Content';
                rowUid = parentsUntil(<Element>e.target, 'e-row') ? parentsUntil(<Element>e.target, 'e-row').getAttribute('data-uid') : null;
                cellColIndex = parentsUntil(<Element>e.target, 'e-rowcell') ? parentsUntil(<Element>e.target, 'e-rowcell').getAttribute('aria-colindex') : null;
                cellColIndex = parseInt(cellColIndex, 10);
            }
            if (target === 'Header' || target === 'Content' || target === 'Pager' || target === 'Edit') {
                this.dotNetRef.invokeMethodAsync('MouseDownHandler', target, cellUid, rowUid, cellColIndex);
            }
        }
    }

    public preventClickOnDrag(e: MouseEventArgs): void {
        if (this._mouseDownX != null && this._mouseDownY != null && this.preventSaveCellOnDragRelease) {
            const positionChanged: boolean = this._mouseDownX != null && this._mouseDownY != null &&
                (this._mouseDownX !== e.clientX || this._mouseDownY !== e.clientY);
            this._mouseDownX = null;
            this._mouseDownY = null;
            if (positionChanged) {
                e.stopPropagation();
            }
        }
    }

    public gridFocus(e: FocusEvent): void { //new
        if (!isNullOrUndefined(this.element.querySelector('.e-gridform')) &&
            this.element.querySelector('.e-gridform').classList.contains('e-editing')) { return; }
        this.dotNetRef.invokeMethodAsync('GridFocus', e);
    }

    public keyActionHandler(e: KeyboardEventArgs): void {
        const elementTag: string = (e.target as HTMLElement).tagName;
        let isSelectTag: boolean = false;
        let isGridEditForm: boolean = false;
        isSelectTag = !isNullOrUndefined(this.element.querySelector('.e-gridform')) && this.element.querySelector('.e-gridform').classList.contains('e-editing') && elementTag === 'SELECT';
        if (e.action === 'pageUp' || e.action === 'pageDown' || e.action === 'ctrlAltPageUp'
            || e.action === 'ctrlAltPageDown' || e.action === 'altPageUp' || e.action === 'altPageDown'
            || (e.action === 'altDownArrow' && !isSelectTag) || e.action === 'ctrlPlusP') {
            e.preventDefault();
        }
        const allow: boolean = this.isPopUpOpened(e);
        const target : Element = e.target as Element;
        const inputElement: Element = parentsUntil(target, 'e-autocomplete');
        const targetElement : Element | null = target.hasAttribute('aria-expanded') ? target : parentsUntil(target, 'e-ddl');
        const isDropdownElement : boolean = targetElement?.getAttribute('aria-expanded') === 'true';
        if (!isNullOrUndefined(inputElement)) {
            const id: string = inputElement.id + '_popup';
            if (!isNullOrUndefined(document.getElementById(id))) {
                return;
            }
        }
        if (parentsUntil(<Element>e.target, 'e-unboundcelldiv') && e.action === 'enter' && (<Element>e.target).classList.contains('e-Savebutton')) {
            return;
        }
        if (!isNullOrUndefined(this.element.querySelector('tr.e-showAddNewRow')) && (parentsUntil(<Element>e.target, 'e-filterbarcell') || (<Element>e.target).classList.contains('e-searchinput'))) {
            return;
        }
        const gridForms: NodeListOf<HTMLElement> = this.element.querySelectorAll('.e-gridform');

        if (!isNullOrUndefined(gridForms)) {
            gridForms.forEach(function (gridForm: HTMLElement): void {
                if (gridForm.classList.contains('e-editing') || (gridForm.classList.contains('e-adding') && parentsUntil(<Element>e.target, 'e-showAddNewRow'))) {
                    isGridEditForm = true;
                }
            });
        }
        if (((e.action === 'enter' && elementTag !== 'TEXTAREA'
            && !(e.target as HTMLElement).classList.contains('e-find-input')) ||
            (e.action === 'ctrlPlusEnter' && elementTag === 'TEXTAREA')) &&
            isGridEditForm && this.options.editMode !== 'Batch' &&
            ((allow && !isDropdownElement) || !allow)) {
            if (this.options.showAddNewRow && this.options.showColumnChooser) {
                e.stopPropagation();
            }
            setTimeout(() => {
                (e.target as HTMLElement).blur();
                this.dotNetRef.invokeMethodAsync('EndEdit', {
                    key: e.key,
                    code: e.code,
                    ctrlKey: e.ctrlKey,
                    shiftKey: e.shiftKey,
                    altKey: e.altKey
                });
            }, 40);
        }
    }

    private updateFixedcolumns(): void {
        const cols: Column[] = this.columnModel;
        const FixedColumns: Column[] = [];
        const unFixedColumns: Column[] = [];

        // Separate columns into locked and unlocked
        cols.forEach((column: Column) => {
            (column.fixedColumn ? FixedColumns : unFixedColumns).push(column);
        });

        // Combine locked and unlocked columns, locked columns first
        this.columnModel = FixedColumns.concat(unFixedColumns);
    }

    public checkFixedColumns(columns: Column[]): boolean {
        for (const column of columns) {
            if (column.columns && this.checkFixedColumns(column.columns)) {
                return true; // Stop further iteration if a locked column is found
            } else if (column.fixedColumn) {
                return true; // Return immediately when a locked column is found
            }
        }
        return false; // No locked column found
    }

    public destroy(): void {
        this.unWireEvents();
        this.frozenDragDropModule.unwireEvents();
        this.virtualContentModule.removeEventListener();
        if (this.domVirtualScrollCleanup) {
            this.domVirtualScrollCleanup();
        }
        if (this.domVirtualMaxHeightCleanup) {
            this.domVirtualMaxHeightCleanup();
        }
        this.addScrollEvents(false);
        this.editedCellIndex = null;
        this.toolTipModule.destroy();
        this.keyModule.destroy();
        this.columnChooserModule.removeMediaListener();
        this.selectionModule.removeEventListener();
        this.rowDragAndDropModule.destroy();
        this.headerDragDrop.destroy();
        this.scrollModule.destroy();
        this.infiniteScrollModule.destroy();
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (window as any).sfBlazor.disposeWindowsInstance(this.dataId);
    }
    /**
     * Retrieves the column indexes currently in view.
     *
     * @private
     * @returns {number[]} The column indexes currently in view.
     */
    public getColumnIndexesInView(): number[] {
        return this.inViewIndexes;
    }

    /**
     * Sets the column indexes to be displayed in view.
     *
     * @private
     * @param {number[]} indexes - The new column indexes to set.
     * @returns {void}
     */
    public setColumnIndexesInView(indexes: number[]): void {
        this.inViewIndexes = indexes;
    }

    public getRowHeight(isVirtual?: boolean): number {
        let rowHeight: number = this.options.rowHeight ? this.options.rowHeight : getRowHeight(this.element);
        if (isVirtual && this.options.rowHeight === 0 && this.getContent().querySelectorAll('tr')[0].classList.contains('e-row') && this.options.enableVirtualMaskRow && this.options.overscanCount === 0) {
            const rows: NodeListOf<Element> = this.getContent().querySelectorAll('.e-row:not(.e-masked-row)');
            if (rows.length > 0 && !this.options.isRenderedFromTreeGrid) {
                rowHeight = Math.round(rows[0].getBoundingClientRect().height);
            }
        }
        return rowHeight;
    }

    private clientActions(): InitModulesResults {
        const gridInitializeResults: InitModulesResults = {
            rowHeight: null,
            isMacDevice : null,
            indentWidth: null,
            isRowDragCell: null
        };
        if ((this.options.enableVirtualization || this.options.enableColumnVirtualization) && (this.options.pageSize === 12 || this.options.width === 'auto' || this.options.width === '100%')) {
            gridInitializeResults.rowHeight = this.getRowHeight();
            this.virtualContentModule.ensurePageSize();
        }
        if (this.getColumns().some((col: Column) => col.hideAtMedia !== '')) {
            this.columnChooserModule.setMediaColumns();
        }
        return gridInitializeResults;
    }
    
    public print(headerId?: string, footerId?: string): void {
        this.removeColGroup();
        const printContainer: HTMLElement = document.createElement('div');
        if (headerId) {
            const headerElement: HTMLElement = document.getElementById(headerId) as HTMLElement;
            if (!isNullOrUndefined(headerElement)) {
                const headerClone: HTMLElement = headerElement.cloneNode(true) as HTMLElement;
                headerClone.classList.add('e-print-header');
                printContainer.appendChild(headerClone);
            }
        }
        printContainer.appendChild(this.element.cloneNode(true));
        if (footerId) {
            const footerElement: HTMLElement = document.getElementById(footerId) as HTMLElement;
            if (!isNullOrUndefined(footerElement)) {
                const footerClone: HTMLElement = footerElement.cloneNode(true) as HTMLElement;
                footerClone.classList.add('e-print-footer');
                printContainer.appendChild(footerClone);
            }
        }
        const printWind: Window = window.open('', 'print', 'height=' + window.outerHeight + ',width=' + window.outerWidth + ',tabbar=no' );
        printWind.moveTo(0, 0);
        printWind.resizeTo(screen.availWidth, screen.availHeight);
        print(printContainer, printWind);
    }

    private removeColGroup(): void {
        const depth: number = this.options.groupCount;
        const element: HTMLElement = this.element;
        const id: string = '#' + this.element.id;
        if (!depth) {
            return;
        }
        const groupCaption: NodeList = element.querySelectorAll('.e-groupcaption');
        const colSpan: string | null = (<HTMLElement>groupCaption[depth - 1]).getAttribute('colspan');
        for (let i: number = 0; i < groupCaption.length; i++) {
            (<HTMLElement>groupCaption[parseInt(i.toString(), 10)]).setAttribute('colspan', colSpan);
        }
        const colGroups: NodeList = element.querySelectorAll(`colgroup${id}colGroup`);
        const contentColGroups: NodeList = element.querySelector('.e-content').querySelectorAll('colgroup');
        const headerColGroups: NodeList = element.querySelector('.e-headercontent').querySelectorAll('colgroup');
        this.hideColGroup(colGroups, depth);
        this.hideColGroup(contentColGroups, depth);
        this.hideColGroup(headerColGroups, depth);
    }

    private hideColGroup(colGroups: NodeList, depth: number): void {
        for (let i: number = 0; i < colGroups.length; i++) {
            for (let j: number = 0; j < depth; j++) {
                (<HTMLElement>(<HTMLElement>colGroups[parseInt(i.toString(), 10)]).children[parseInt(j.toString(), 10)]).style.display = 'none';
            }
        }
    }

    private isLandScapeMobile(): boolean {
        const isLandscape: boolean = window.matchMedia("(orientation: landscape)").matches;
        const isSmallScreen: boolean = window.matchMedia("(max-width: 1024px)").matches;
        const hasTouch: boolean = window.matchMedia("(pointer: coarse)").matches;
        return isLandscape && isSmallScreen && hasTouch;
    }

    /**
     * Checks if the current browser is Safari on a macOS device.
     *
     * @returns {boolean} - Returns `true` if the browser is Safari and the operating system is macOS, otherwise `false`.
     */
    public isMacSafariBrowser(): boolean {
        const userAgent: string = navigator.userAgent;
        const isSafariBrowser: boolean = /^((?!chrome|android).)*safari/i.test(userAgent); // Check the browser is Safari
        return this.isMacOS() && isSafariBrowser;
    }

    /**
     * Checks if the current operating system is macOS.
     *
     * @returns {boolean} - Returns `true` if the operating system is macOS, otherwise `false`.
     */
    public isMacOS(): boolean {
        const userAgent: string = navigator.userAgent;
        const isMacOS: boolean = userAgent.indexOf('Mac OS') !== -1; // check whether it is on macOS
        return isMacOS;
    }

    /**
     * For internal use only - Get the module name.
     *
     * @private
     * @returns {string} The module name.
     */
    protected getModuleName(): string {
        return 'grid';
    }
}

const gridKeyConfigs: { [x: string]: string } = {
    pageUp: 'pageup',
    pageDown: 'pagedown',
    ctrlAltPageUp: 'ctrl+alt+pageup',
    ctrlAltPageDown: 'ctrl+alt+pagedown',
    altPageUp: 'alt+pageup',
    altPageDown: 'alt+pagedown',
    altDownArrow: 'alt+downarrow',
    altUpArrow: 'alt+uparrow',
    ctrlDownArrow: 'ctrl+downarrow',
    ctrlUpArrow: 'ctrl+uparrow',
    ctrlPlusA: 'ctrl+A',
    ctrlPlusP: 'ctrl+P',
    ctrlPlusC: 'ctrl+C',
    ctrlShiftPlusH: 'ctrl+shift+H',
    enter: 'enter',
    ctrlPlusEnter: 'ctrl+enter'
};
