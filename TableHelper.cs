using System;

using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Utilities.Web.HttpHandlers
{
    /// <summary>
    /// Helper class used when building up the Html tables for display
    /// </summary>
    internal class TableHelper
    {
        /// <summary>
        /// The add cell.
        /// </summary>
        /// <param name="rowToAddCellTo">
        /// The row to add cell to.
        /// </param>
        /// <param name="cellText">
        /// The cell text.
        /// </param>
        /// <returns>
        /// The <see cref="TableCell"/>.
        /// </returns>
        public static TableCell AddCell(TableRow rowToAddCellTo, string cellText)
        {
            var cell = new TableCell();

            cell.Text = cellText;
            rowToAddCellTo.Cells.Add(cell);

            return cell;
        }

        /// <summary>
        /// The add cell.
        /// </summary>
        /// <param name="rowToAddCellTo">
        /// The row to add cell to.
        /// </param>
        /// <param name="cellText">
        /// The cell text.
        /// </param>
        /// <param name="hyperLink">
        /// The hyper link.
        /// </param>
        /// <returns>
        /// The <see cref="TableCell"/>.
        /// </returns>
        public static TableCell AddCell(TableRow rowToAddCellTo, string cellText, string hyperLink)
        {
            var cell = new TableCell();
            var anchor = new HtmlAnchor();

            anchor.HRef = hyperLink;
            anchor.InnerText = cellText;
            cell.Controls.Add(anchor);

            rowToAddCellTo.Cells.Add(cell);

            return cell;
        }

        /// <summary>
        /// The add header cell.
        /// </summary>
        /// <param name="rowToAddCellTo">
        /// The row to add cell to.
        /// </param>
        /// <param name="cellText">
        /// The cell text.
        /// </param>
        /// <returns>
        /// The <see cref="TableCell"/>.
        /// </returns>
        public static TableCell AddHeaderCell(TableRow rowToAddCellTo, string cellText)
        {
            var cell = new TableHeaderCell();

            cell.Text = cellText;
            rowToAddCellTo.Cells.Add(cell);
            cell.HorizontalAlign = HorizontalAlign.Left;
            return cell;
        }

        public static Table CreateTable()
        {
            var table = new Table();

            table.BorderStyle = BorderStyle.Solid;
            table.BorderWidth = Unit.Pixel(1);
            table.Width = Unit.Percentage(100);
            table.CellPadding = 0;
            table.CellSpacing = 0;
            return table;
        }
    }
}
