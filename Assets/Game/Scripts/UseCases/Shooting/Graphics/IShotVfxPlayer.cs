using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bw.UseCases.Shooting.Graphics
{
    public interface IShotVfxPlayer
    {
        public UniTaskVoid Play(Vector3 from, Vector3 to);
    }
}