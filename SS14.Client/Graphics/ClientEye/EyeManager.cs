﻿using System;
using SS14.Client.Interfaces;
using SS14.Client.Interfaces.Graphics.ClientEye;
using SS14.Client.Utility;
using SS14.Shared.Interfaces.Map;
using SS14.Shared.IoC;
using SS14.Shared.Map;
using SS14.Shared.Maths;

namespace SS14.Client.Graphics.ClientEye
{
    public sealed class EyeManager : IEyeManager, IDisposable
    {
        // If you modify this make sure to edit the value in the SS14.Shared.Audio.AudioParams struct default too!
        // No I can't be bothered to make this a shared constant.
        public const int PIXELSPERMETER = 32;

        [Dependency] readonly ISceneTreeHolder sceneTree;

        // We default to this when we get set to a null eye.
        private FixedEye defaultEye;

        private IEye currentEye;

        public IEye CurrentEye
        {
            get => currentEye;
            set
            {
                if (currentEye == value)
                {
                    return;
                }

                currentEye.GodotCamera.Current = false;
                if (value != null)
                {
                    currentEye = value;
                }
                else
                {
                    currentEye = defaultEye;
                }

                currentEye.GodotCamera.Current = true;
            }
        }

        public MapId CurrentMap => currentEye.MapId;

        public Box2 GetWorldViewport()
        {
            var vpSize = sceneTree.SceneTree.Root.Size.Convert();

            var topLeft = ScreenToWorld(Vector2.Zero);
            var topRight = ScreenToWorld(new Vector2(vpSize.X, 0));
            var bottomRight = ScreenToWorld(vpSize);
            var bottomLeft = ScreenToWorld(new Vector2(0, vpSize.Y));

            var left = MathHelper.Min(topLeft.X, topRight.X, bottomRight.X, bottomLeft.X);
            var bottom = MathHelper.Min(topLeft.Y, topRight.Y, bottomRight.Y, bottomLeft.Y);
            var right = MathHelper.Max(topLeft.X, topRight.X, bottomRight.X, bottomLeft.X);
            var top = MathHelper.Max(topLeft.Y, topRight.Y, bottomRight.Y, bottomLeft.Y);

            return new Box2(left, bottom, right, top);
        }

        public void Initialize()
        {
            defaultEye = new FixedEye();
            currentEye = defaultEye;
            currentEye.GodotCamera.Current = true;
        }

        public void Dispose()
        {
            defaultEye.Dispose();
        }

        public Vector2 WorldToScreen(Vector2 point)
        {
            var transform = sceneTree.WorldRoot.GetViewportTransform();
            return transform.Xform(point.Convert() * PIXELSPERMETER * new Godot.Vector2(1, -1)).Convert();
        }

        public ScreenCoordinates WorldToScreen(GridLocalCoordinates point)
        {
            return new ScreenCoordinates(WorldToScreen(point.Position));
        }

        public GridLocalCoordinates ScreenToWorld(ScreenCoordinates point)
        {
            return ScreenToWorld(point.Position);
        }

        public GridLocalCoordinates ScreenToWorld(Vector2 point)
        {
            var matrix = Matrix3.Invert(MatrixViewPortTransform(sceneTree));
            var vec3 = new Vector3(point.X, point.Y, 1);
            matrix.Transform(ref vec3);
            var vec2 = new Vector2(vec3.X, vec3.Y);
            var worldPos = vec2 / PIXELSPERMETER * new Vector2(1, -1);
            var grid = IoCManager.Resolve<IMapManager>().GetMap(currentEye.MapId).FindGridAt(worldPos);
            // TODO: This is incorrect. It assumes the grid has an identity transform.
            return new GridLocalCoordinates(worldPos, grid);
        }

        private static Matrix3 MatrixViewPortTransform(ISceneTreeHolder sceneTree)
        {
            var transform = sceneTree.WorldRoot.GetViewportTransform();
            return sceneTree.WorldRoot.GetViewportTransform().Convert();
        }
    }
}
