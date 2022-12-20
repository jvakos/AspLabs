using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components.Rendering;

namespace Microsoft.AspNetCore.Components.QuickGrid;

public class NamedColumn<TGridItem> : ColumnBase<TGridItem>, ISortBuilderColumn<TGridItem>
{

    private PropertyInfo? lastAssignedPropertyInfo;
    private Func<TGridItem, string?>? _cellTextFunc;
    private GridSort<TGridItem>? _sortBuilder;


    /// <summary>
    /// Defines the property name of the value to be displayed in this column's cells.
    /// </summary>
    [Parameter, EditorRequired] public string PropertyName { get; set; } = default!;

    /// <summary>
    /// Optionally specifies a format string for the value.
    ///
    /// Using this requires the property type to implement <see cref="IFormattable" />.
    /// </summary>
    [Parameter] public string? Format { get; set; }

    GridSort<TGridItem>? ISortBuilderColumn<TGridItem>.SortBuilder => _sortBuilder;



    protected override void OnParametersSet()
    {
        if (lastAssignedPropertyInfo?.Name != PropertyName)
        {
            lastAssignedPropertyInfo = typeof(TGridItem).GetProperty(PropertyName);
            if (lastAssignedPropertyInfo != null) {
                Type propType = lastAssignedPropertyInfo.PropertyType;

                if (!string.IsNullOrEmpty(Format))
                {
                    var nullableUnderlyingTypeOrNull = Nullable.GetUnderlyingType(propType);

                    if (!typeof(IFormattable).IsAssignableFrom(nullableUnderlyingTypeOrNull ?? propType))
                    {
                        throw new InvalidOperationException($"A '{nameof(Format)}' parameter was supplied, but the type '{propType}' does not implement '{typeof(IFormattable)}'.");
                    }
                    _cellTextFunc = item => (lastAssignedPropertyInfo.GetValue(item) as IFormattable)?.ToString(Format, null);

                }
                else {
                    _cellTextFunc = item => lastAssignedPropertyInfo.GetValue(item)?.ToString();
                }
            }
            //_sortBuilder =  GridSort<TGridItem>.ByAscending( Expression<Func<TGridItem, U>>);
        }

        if (Title is null && lastAssignedPropertyInfo != null)
        {
            Title = lastAssignedPropertyInfo.Name;
        }
    }

    protected internal override void CellContent(RenderTreeBuilder builder, TGridItem item)
       => builder.AddContent(0, _cellTextFunc!(item));

}

