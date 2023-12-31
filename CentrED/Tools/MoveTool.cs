﻿using CentrED.Map;
using CentrED.UI;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using static CentrED.Application;
using Vector2 = System.Numerics.Vector2;

namespace CentrED.Tools;

public class MoveTool : BaseTool
{
    public override string Name => "Move";
    public override Keys Shortcut => Keys.F3;

    private int _xDelta;
    private int _yDelta;

    private Vector2 _dragDelta = Vector2.Zero;
    private int _xDragDelta;
    private int _yDragDelta;

    internal override void Draw()
    {
        var buttonSize = new Vector2(19, 19);
        var spacing = new Vector2(4, 4);
        var totalWidth = 3 * buttonSize.X + 2 * spacing.X;
        var xOffset = (ImGui.GetContentRegionAvail().X - totalWidth) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + xOffset);
        ImGui.BeginGroup();
        ImGui.PushButtonRepeat(true);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, spacing);
        var startPos = ImGui.GetCursorPos();
        if (ImGui.Button("##x1", buttonSize))
        {
            _xDelta--;
        }
        ImGui.SameLine();
        if (ImGui.ArrowButton("up", ImGuiDir.Up))
        {
            _xDelta--;
            _yDelta--;
        }
        ImGui.SameLine();
        if (ImGui.Button("##y1", buttonSize))
        {
            _yDelta--;
        }
        if (ImGui.ArrowButton("left", ImGuiDir.Left))
        {
            _xDelta--;
            _yDelta++;
        }
        ImGui.SameLine();
        ImGui.PopButtonRepeat();
        if (ImGui.Button("?", buttonSize))
        {
            if (_dragDelta == Vector2.Zero)
            {
                _xDelta = 0;
                _yDelta = 0;
            }
        }
        if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            _dragDelta = ImGui.GetMouseDragDelta();
            var newVec = MapManager.ScreenToMapCoordinates(_dragDelta.X / 20, _dragDelta.Y / 20);
            _xDragDelta = (int)newVec.X;
            _yDragDelta = (int)newVec.Y;
        }
        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _dragDelta != Vector2.Zero)
        {
            _xDelta += _xDragDelta;
            _yDelta += _yDragDelta;
            _dragDelta = Vector2.Zero;
            _xDragDelta = 0;
            _yDragDelta = 0;
        }
        var xTempDelta = _xDelta + _xDragDelta;
        var yTempDelta = _yDelta + _yDragDelta;
        
        UIManager.Tooltip("Drag Me\n" + "Click to reset");
        ImGui.SameLine();
        ImGui.PushButtonRepeat(true);
        if (ImGui.ArrowButton("right", ImGuiDir.Right))
        {
            _xDelta++;
            _yDelta--;
        }
        if (ImGui.Button("##y2", buttonSize))
        {
            _yDelta++;
        }
        ImGui.SameLine();
        if (ImGui.ArrowButton("down", ImGuiDir.Down))
        {
            _xDelta++;
            _yDelta++;
        }
        ImGui.SameLine();
        if (ImGui.Button("##x2", buttonSize))
        {
            _xDelta++;
        }
        ImGui.PopButtonRepeat();
        ImGui.PopStyleVar();
        var endPos = ImGui.GetCursorPos();
        var style = ImGui.GetStyle();
        var framePadding = style.FramePadding;
        if(xTempDelta < 0 )
        {
            ImGui.SetCursorPos(startPos + framePadding);
            ImGui.Text($"{-xTempDelta}");
        }
        if(yTempDelta < 0 )
        {
            ImGui.SetCursorPos(startPos + new Vector2((buttonSize.X + spacing.X) * 2 , 0) + framePadding);
            ImGui.Text($"{-yTempDelta}");
        }
        if(yTempDelta > 0 )
        {
            ImGui.SetCursorPos(startPos + new Vector2(0, (buttonSize.Y + spacing.Y) * 2) + framePadding);
            ImGui.Text($"{yTempDelta}");
        }
        if(xTempDelta > 0 )
        {
            ImGui.SetCursorPos(startPos + (buttonSize + spacing) * 2 + framePadding);
            ImGui.Text($"{xTempDelta}");
        }
        ImGui.SetCursorPos(endPos);
        ImGui.EndGroup();
        if (ImGui.Button("Inverse"))
        {
            _xDelta = -_xDelta;
            _yDelta = -_yDelta;
        }
        
        ImGui.InputInt("X", ref _xDelta);
        ImGui.InputInt("Y", ref _yDelta);
    }

    protected override void GhostApply(TileObject? o)
    {
        if (o is StaticObject so)
        {
            so.Alpha = 0.3f;
            var newTile = new StaticTile
            (
                so.StaticTile.Id,
                (ushort)(so.StaticTile.X + _xDelta),
                (ushort)(so.StaticTile.Y + _yDelta),
                so.StaticTile.Z,
                so.StaticTile.Hue
            );
            CEDGame.MapManager.GhostStaticTiles.Add(o, new StaticObject(newTile));
        }
    }

    protected override void GhostClear(TileObject? o)
    {
        if (o is StaticObject so)
        {
            so.Alpha = 1f;
            CEDGame.MapManager.GhostStaticTiles.Remove(o);
        }
    }

    protected override void Apply(TileObject? o)
    {
        if (o is StaticObject so)
        {
            var ghostTile = CEDGame.MapManager.GhostStaticTiles[o];
            so.StaticTile.UpdatePos(ghostTile.Tile.X, ghostTile.Tile.Y, so.StaticTile.Z);
        }
    }
}