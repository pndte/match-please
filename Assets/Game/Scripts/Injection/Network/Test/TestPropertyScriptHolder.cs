using JetBrains.Lifetimes;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Network
{
    public class TestPropertyScriptHolder : MonoBehaviour
    {
        public TestPropertyScript TestPropertyScript;
        
        [Inject]
        private void Construct(Lifetime lifetime, TestPropertyScript testPropertyScript)
        {
            TestPropertyScript = testPropertyScript;
            testPropertyScript.Health.Advise(lifetime, i => Debug.Log("New health value: " + i));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                TestPropertyScript.Health.Value -= 10;
            
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                TestPropertyScript.Health.Value += 10;
            }
        }
    }
}