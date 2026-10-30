using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Blazor.Data;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Executes the event-driven data pipeline for read and CRUD operations.
    /// Handles conversion of internal query metadata to public grid event DTOs and invocation of grid event callbacks.
    /// </summary>
    /// <typeparam name="T">The grid row type.</typeparam>
    internal static class EventDrivenDataHandler<T>
    {
        /// <summary>
        /// Determines whether the grid is configured to use event-driven data loading.
        /// </summary>
        /// <param name="grid">The grid whose data-loading event is evaluated.</param>
        /// <returns><see langword="true"/> when an <c>OnDataLoad</c> handler is registered.</returns>
        internal static bool IsEventDriven(SfGrid<T> grid)
        {
            return grid.GridEvents?.OnDataLoad.HasDelegate == true;
        }

        // ========== QUERY METADATA CONVERSION ==========

        /// <summary>
        /// Converts internal WhereFilter data-layer objects to public FilterRequest event DTOs.
        /// </summary>
        internal static List<FilterRequest>? ConvertFilters(List<WhereFilter>? filters)
        {
            if (filters == null)
            {
                return null;
            }

            return filters.Select(filter => new FilterRequest
            {
                Field = filter.Field,
                IgnoreCase = filter.IgnoreCase,
                IgnoreAccent = filter.IgnoreAccent,
                IsComplex = filter.IsComplex,
                Operator = filter.Operator,
                Condition = filter.Condition,
                Value = filter.value,
                Predicates = filter.predicates == null ? null : ConvertFilters(filter.predicates),
                ColumnType = filter.ColumnType
            }).ToList();
        }

        /// <summary>
        /// Converts internal Sort data-layer objects to public SortRequest event DTOs.
        /// </summary>
        internal static List<SortRequest>? ConvertSorts(List<Sort>? sorts)
        {
            if (sorts == null)
            {
                return null;
            }

            return sorts.Select(sort => new SortRequest
            {
                Name = sort.Name,
                Direction = sort.Direction,
                Comparer = sort.Comparer
            }).ToList();
        }

        /// <summary>
        /// Converts internal SearchFilter data-layer objects to public SearchRequest event DTOs.
        /// </summary>
        internal static List<SearchRequest>? ConvertSearches(List<SearchFilter>? searches)
        {
            if (searches == null)
            {
                return null;
            }

            return searches.Select(search => new SearchRequest
            {
                Fields = search.Fields,
                Key = search.Key,
                Operator = search.Operator,
                IgnoreCase = search.IgnoreCase,
                IgnoreAccent = search.IgnoreAccent
            }).ToList();
        }

        /// <summary>
        /// Converts internal Aggregate data-layer objects to public AggregateRequest event DTOs.
        /// </summary>
        internal static List<AggregateRequest>? ConvertAggregates(List<Data.Aggregate>? aggregates)
        {
            if (aggregates == null)
            {
                return null;
            }

            return aggregates.Select(aggregate => new AggregateRequest
            {
                Field = aggregate.Field,
                Type = aggregate.Type
            }).ToList();
        }

        // ========== EVENT INVOCATION ==========

        /// <summary>
        /// Invokes the <see cref="GridEvents{T}.OnDataLoad"/> callback when it is assigned.
        /// Builds the event args from query metadata internally and returns the populated result.
        /// </summary>
        /// <param name="query">The query object containing filter, sort, search, and aggregate metadata.</param>
        /// <returns>The populated GridLoadEventArgs or null if the event was not invoked.</returns>
        internal static async Task<GridLoadEventArgs<T>?> InvokeDataRequestAsync(Query query)
        {
            // Build event args from query metadata - single source of truth
            var eventArgs = new GridLoadEventArgs<T>
            {
                Request = new DataRequest()
                {
                    Skip = query.Queries.Skip,
                    Take = query.Queries.Take,
                    Filter = ConvertFilters(query.Queries.Where),
                    Sort = ConvertSorts(query.Queries.Sorted),
                    Group = query.Queries.Group,
                    Search = ConvertSearches(query.Queries.Search),
                    Select = query.Queries.Select,
                    Expand = query.Queries.Expand,
                    Aggregates = ConvertAggregates(query.Queries.Aggregates),
                }
            };
            return eventArgs;
        }
    }
}