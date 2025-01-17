using UnityEngine;

namespace ITHappy
{
    public class Wheel : MonoBehaviour, ICarPart
    {
        [SerializeField]
        private bool m_Inverse;
        [SerializeField]
        private float m_Radius = 0.45f;

        public void Move(float delta)
        {
            var angularDelta = delta / m_Radius * Mathf.Rad2Deg;
            var euler = Vector3.right * angularDelta;

            if(m_Inverse)
            {
                transform.Rotate(-euler, Space.Self);
            }
            else
            {
                transform.Rotate(euler, Space.Self);
            }
        }
    }
}