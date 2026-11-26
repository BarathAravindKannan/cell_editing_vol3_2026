using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Blazor.Data;

namespace Syncfusion.Blazor.Grids.Internal
{
    internal partial class DomVirtualHandler<T>
    {
        #region Group State

        private List<GroupedDataItem> _groupEntries = new();

        // Stable caption uid per group path key, so a recycled caption row keeps its @key and focus identity.
        private readonly Dictionary<string, string> _captionUids = new(StringComparer.Ordinal);

        // Lazy groups whose children are being fetched (prevents a second request while the user clicks again or scrolls).
        private readonly HashSet<string> _loadingGroups = new(StringComparer.Ordinal);

        private List<GridAggregate> _footerAggregates = new();
        private string _groupColumnsSignature = string.Empty;
        private bool? _defaultExpand;

        /// <summary>
        /// True while the rendered data is grouped. The renderer then windows <see cref="GroupEntryCount"/> entries instead of records.
        /// </summary>
        internal bool IsGroupMode { get; private set; }

        internal int GroupEntryCount => _groupEntries.Count;

        private bool IsLazyGrouping => _parent.GroupSettings?.EnableLazyLoading == true;

        internal static bool IsGroupedSource(SfGrid<T> grid, IEnumerable<object>? source)
        {
            return grid.AllowGrouping && grid.GroupSettings?.Columns?.Length > 0 && source != null && source.FirstOrDefault() is Group<T>;
        }

        /// <summary>
        /// Flattens the grouped data into visible entries. Cost is O(visible rows); collapsed groups contribute only their caption,
        /// using <c>CountItems</c> to keep the data ordinals of the following groups stable.
        /// </summary>
        internal void BuildGroupEntries(IEnumerable<object>? source)
        {
            IsGroupMode = IsGroupedSource(_parent, source);
            if (!IsGroupMode)
            {
                ResetGroupState();
                return;
            }
            string signature = string.Join(",", _parent.GroupSettings!.Columns!);
            if (!string.Equals(signature, _groupColumnsSignature, StringComparison.Ordinal))
            {
                // Grouped columns changed: old keys no longer describe the same groups.
                _groupColumnsSignature = signature;
                _parent.GroupStates.Clear();
                _captionUids.Clear();
                _defaultExpand = null;
            }
            _parent.VirtualScrollModule?.UpdateColumnVisibility();
            _footerAggregates = GetFooterAggregates();
            List<GroupedDataItem> entries = new();
            int dataIndex = 0;
            GroupFlattenMode mode = FlattenMode;
            foreach (object item in source!)
            {
                if (item is Group<T> group)
                {
                    Grouping<T>.FlattenGroup(entries, group, mode, 0, string.Empty, null, true, ref dataIndex);
                }
            }
            _groupEntries = entries;
        }

        internal void ResetGroupState()
        {
            IsGroupMode = false;
            _groupEntries = new();
            _loadingGroups.Clear();
        }

        private bool DefaultExpand => _defaultExpand ?? !IsLazyGrouping;

        // Expand state lives in SfGrid.GroupStates, the store PersistGroupState already uses, so it survives recycling, paging,
        // sorting and filtering and Grouping<T> clears it on group / ungroup.
        private bool IsGroupExpanded(string key) => _parent.GroupStates.TryGetValue(key, out bool expanded) ? expanded : DefaultExpand;

        // Visible rows only; captions get their stable uid, data and footer rows need none (they get one when generated).
        private GroupFlattenMode FlattenMode => new(true, _footerAggregates.Count, IsGroupExpanded, key => key == null ? null : CaptionUid(key));

        /// <summary>
        /// One footer row per aggregate that passes <see cref="GroupModelGenerator{T}.IsVisibleGroupFooter"/>.
        /// </summary>
        private List<GridAggregate> GetFooterAggregates()
        {
            List<GridAggregate> result = new();
            if (_parent.Aggregates == null || _parent.Aggregates.Count == 0)
            {
                return result;
            }
            HashSet<string> visibleColumns = new(GridUtils.GetColumns(_parent).Where(col => col.Visible).Select(col => col.Field), StringComparer.Ordinal);
            foreach (GridAggregate aggregate in _parent.Aggregates)
            {
                if (GroupModelGenerator<T>.IsVisibleGroupFooter(aggregate, visibleColumns))
                {
                    result.Add(aggregate);
                }
            }
            return result;
        }

        #endregion

        #region Row Generation

        /// <summary>
        /// Generates the rows of the entries <c>[start, start + count)</c> through <see cref="GroupModelGenerator{T}.AddGroupedDataRow"/>,
        /// the per-item generator that row virtualization's <c>GroupGeneratorRows</c> uses; only the rendered window is passed, so only those rows are created.
        /// </summary>
        internal List<Row<object>> GenerateGroupEntryRows(int start, int count)
        {
            GroupModelGenerator<T> generator = new(_parent);
            List<Row<object>> rows = new(count);
            var captionRowsByUid = new Dictionary<string, (Row<object> Row, Group<T> Data)>();
            for (int i = start; i < start + count; i++)
            {
                GroupedDataItem entry = _groupEntries[i];
                generator.AddGroupedDataRow(
                    new GroupModelGenerator<T>.GroupedDataParams(entry.Item!, entry.Uid, entry.ParentUid!, entry.Index, entry.RowIndex, entry.Indent,
                        entry.IsExpand, entry.Visible, entry.IsSelected, entry.IsFooterRow),
                    rows, null, captionRowsByUid, entry.IsFooterRow ? _footerAggregates[entry.AggregateIndex] : null);
            }
            return rows;
        }

        private string CaptionUid(string key)
        {
            if (!_captionUids.TryGetValue(key, out string? uid))
            {
                uid = _parent.GetUid("grid-row");
                _captionUids[key] = uid;
            }
            return uid;
        }

        /// <summary>
        /// Entry index of the visible data row with the given data ordinal, or -1 when its group is collapsed. Entry data ordinals
        /// never decrease along the list, so this is a binary search (O(log n)).
        /// </summary>
        private int DataEntryIndex(int dataIndex)
        {
            int low = 0;
            int high = _groupEntries.Count;
            while (low < high)
            {
                int mid = (low + high) >> 1;
                if (_groupEntries[mid].Index < dataIndex)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }
            for (int i = low; i < _groupEntries.Count && _groupEntries[i].Index == dataIndex; i++)
            {
                if (!_groupEntries[i].IsCaptionRow && !_groupEntries[i].IsFooterRow)
                {
                    return i;
                }
            }
            return -1;
        }

        private int FindCaptionEntry(string uid)
        {
            for (int i = 0; i < _groupEntries.Count; i++)
            {
                if (_groupEntries[i].IsCaptionRow && string.Equals(_groupEntries[i].Uid, uid, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region Expand / Collapse

        /// <summary>
        /// Expands or collapses one group: the entry list is spliced in place (children removed or inserted after the caption),
        /// lazy children are fetched on the first expand, and the window is re-rendered once.
        /// </summary>
        internal async Task ToggleGroupAsync(Row<object> captionRow)
        {
            int index = captionRow.Uid != null ? FindCaptionEntry(captionRow.Uid) : -1;
            if (index < 0)
            {
                return;
            }
            GroupedDataItem caption = _groupEntries[index];
            Group<T> group = (Group<T>)caption.Item!;
            bool expand = !IsGroupExpanded(caption.Key!);
            if (expand && !Grouping<T>.HasItems(group) && IsLazyGrouping)
            {
                if (!_loadingGroups.Add(caption.Key!))
                {
                    return;
                }
                try
                {
                    IEnumerable children = await _parent.GroupModule!.FetchLazyGroupChildrenAsync(group, AncestorsOf(index)).ConfigureAwait(true);
                    group.Items = children;
                }
                finally
                {
                    _loadingGroups.Remove(caption.Key!);
                }
                // The list may have been rebuilt (data change) while the request ran.
                index = captionRow.Uid != null ? FindCaptionEntry(captionRow.Uid) : -1;
                if (index < 0)
                {
                    return;
                }
            }
            _parent.GroupStates[caption.Key!] = expand;
            caption.IsExpand = expand;
            int removeEnd = SubtreeEnd(index);
            _groupEntries.RemoveRange(index + 1, removeEnd - (index + 1));
            if (expand && Grouping<T>.HasItems(group))
            {
                int dataIndex = caption.Index!.Value;
                List<GroupedDataItem> children = new();
                Grouping<T>.FlattenChildren(children, group, FlattenMode, caption.Indent, caption.Key!, caption.Uid, true, ref dataIndex);
                _groupEntries.InsertRange(index + 1, children);
            }
            captionRow.IsExpand = expand;
            await RefreshGroupWindowAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Expands or collapses every group (lazy groups are expanded only when their children are already loaded; the rest stay
        /// collapsed so expand-all never issues one request per group).
        /// </summary>
        internal async Task ExpandCollapseAllGroupsAsync(bool expand)
        {
            _parent.GroupStates.Clear();
            _defaultExpand = expand;
            BuildGroupEntries(_parent.CurrentViewData);
            await RefreshGroupWindowAsync().ConfigureAwait(true);
        }

        private int SubtreeEnd(int captionIndex)
        {
            int depth = _groupEntries[captionIndex].Indent;
            int end = captionIndex + 1;
            while (end < _groupEntries.Count && _groupEntries[end].Indent > depth)
            {
                end++;
            }
            return end;
        }

        private List<Group<T>> AncestorsOf(int captionIndex)
        {
            List<Group<T>> path = new();
            int depth = _groupEntries[captionIndex].Indent;
            for (int i = captionIndex - 1; i >= 0 && depth > 0; i--)
            {
                if (_groupEntries[i].IsCaptionRow && _groupEntries[i].Indent == depth - 1)
                {
                    path.Insert(0, (Group<T>)_groupEntries[i].Item!);
                    depth--;
                }
            }
            path.Add((Group<T>)_groupEntries[captionIndex].Item!);
            return path;
        }

        private async Task RefreshGroupWindowAsync()
        {
            IDomVirtualContent? content = Content;
            if (content == null)
            {
                return;
            }
            // Entry indexes after the toggled caption shifted: rebuild the window rows (only the window is generated again).
            RetainedRows.Clear();
            // Expanded details inside groups move with their rows (and are hidden in collapsed groups).
            RebuildDetailHeights();
            content.ClearRowCache();
            await content.UpdateRenderedRowsAsync(true).ConfigureAwait(true);
        }

        #endregion
    }
}
