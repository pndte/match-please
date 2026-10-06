using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Camera.View.Follow
{
    public interface ICameraTargets : IViewableList<Transform>
    {
    }

    public sealed class CameraTargets : ViewableList<Transform>, ICameraTargets
    {
    }
}
