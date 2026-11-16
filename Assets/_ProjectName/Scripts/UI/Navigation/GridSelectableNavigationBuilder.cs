using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSelectableNavigationBuilder : SelectableNavigationBuilderBase
{
    [Header("Grid")]
    [SerializeField, Min(0f)] private float rowTolerance = 10f;

    protected override void BuildNavigation(List<Selectable> selectables)
    {
        List<List<Selectable>> rows = BuildRows(selectables);
        if (rows.Count == 0)
            return;

        for (int rowIndex = 0; rowIndex < rows.Count; ++rowIndex)
        {
            List<Selectable> row = rows[rowIndex];

            for (int columnIndex = 0; columnIndex < row.Count; ++columnIndex)
            {
                Selectable current = row[columnIndex];

                Selectable selectLeft = GetLeft(row, columnIndex);
                Selectable selectRight = GetRight(row, columnIndex);
                Selectable selectUp = GetUp(rows, rowIndex, columnIndex);
                Selectable selectDown = GetDown(rows, rowIndex, columnIndex);

                ApplyNavigation(current, CreateNavigation(current, selectUp, selectDown, selectLeft, selectRight));
            }
        }
    }

    private List<List<Selectable>> BuildRows(List<Selectable> selectables)
    {
        List<Selectable> sorted = new List<Selectable>(selectables);

        sorted.Sort((a, b) =>
        {
            Vector2 posA = GetAnchoredPosition(a);
            Vector2 posB = GetAnchoredPosition(b);

            int yCompare = posB.y.CompareTo(posA.y);
            if (yCompare != 0)
                return yCompare;

            return posA.x.CompareTo(posB.x);
        });

        List<List<Selectable>> rows = new List<List<Selectable>>();

        foreach (Selectable selectable in sorted)
        {
            Vector2 position = GetAnchoredPosition(selectable);

            if (rows.Count == 0)
            {
                rows.Add(new List<Selectable> { selectable });
                continue;
            }

            List<Selectable> currentRow = rows[rows.Count - 1];
            Vector2 firstPositionInRow = GetAnchoredPosition(currentRow[0]);

            if (Mathf.Abs(firstPositionInRow.y - position.y) <= rowTolerance)
            {
                currentRow.Add(selectable);
            }
            else
            {
                rows.Add(new List<Selectable> { selectable });
            }
        }

        foreach (List<Selectable> row in rows)
        {
            row.Sort((a, b) =>
            {
                Vector2 posA = GetAnchoredPosition(a);
                Vector2 posB = GetAnchoredPosition(b);
                return posA.x.CompareTo(posB.x);
            });
        }

        return rows;
    }

    private Selectable GetLeft(List<Selectable> row, int columnIndex)
    {
        if (row.Count <= 1)
            return null;

        if (columnIndex > 0)
            return row[columnIndex - 1];

        return LoopMode == SelectableNavigationLoopMode.ClosedLoop ? row[row.Count - 1] : null;
    }

    private Selectable GetRight(List<Selectable> row, int columnIndex)
    {
        if (row.Count <= 1)
            return null;

        if (columnIndex < row.Count - 1)
            return row[columnIndex + 1];

        return LoopMode == SelectableNavigationLoopMode.ClosedLoop ? row[0] : null;
    }

    private Selectable GetUp(List<List<Selectable>> rows, int rowIndex, int columnIndex)
    {
        if (rows.Count <= 1)
            return null;

        int targetRowIndex = rowIndex - 1;

        if (targetRowIndex < 0)
        {
            if (LoopMode != SelectableNavigationLoopMode.ClosedLoop)
                return null;

            targetRowIndex = rows.Count - 1;
        }

        List<Selectable> targetRow = rows[targetRowIndex];
        if (targetRow.Count == 0)
            return null;

        columnIndex = Mathf.Clamp(columnIndex, 0, targetRow.Count - 1);
        return targetRow[columnIndex];
    }

    private Selectable GetDown(List<List<Selectable>> rows, int rowIndex, int columnIndex)
    {
        if (rows.Count <= 1)
            return null;

        int targetRowIndex = rowIndex + 1;

        if (targetRowIndex >= rows.Count)
        {
            if (LoopMode != SelectableNavigationLoopMode.ClosedLoop)
                return null;

            targetRowIndex = 0;
        }

        List<Selectable> targetRow = rows[targetRowIndex];
        if (targetRow.Count == 0)
            return null;

        columnIndex = Mathf.Clamp(columnIndex, 0, targetRow.Count - 1);
        return targetRow[columnIndex];
    }
}