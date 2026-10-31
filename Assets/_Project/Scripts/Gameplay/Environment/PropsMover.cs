using UnityEngine;

namespace DreamVille.Gameplay.Environment
{
    /// <summary>Moves a prop (a balloon) around a loop of points while spinning it; when hit, it floats upwards for a while and then resumes its route.</summary>
    public class PropsMover : MonoBehaviour
    {
        [SerializeField] private Transform _balloon;
        [SerializeField] private Transform[] _points;
        [SerializeField] private float _speed;
        [SerializeField] private float _rotation = 0.5f;

        [SerializeField] private float _floatingTime = 10;

        // Runtime state, not data: GetHit sets it, Update clears it when the float time runs out.
        private bool _isFloating;
        private int _pointIndex;
        private float _timer;

        private void Update()
        {
            if (!_isFloating)
            {
                float distance = Vector3.Distance(_balloon.transform.position, _points[_pointIndex].position);
                if (distance < 0.2f)
                {
                    SetNextPoint();
                }
                _balloon.transform.position = Vector3.MoveTowards(_balloon.transform.position, _points[_pointIndex].position, _speed * Time.deltaTime);
                _balloon.transform.Rotate(0, 0, _rotation);
            }
            else
            {
                _timer -= Time.deltaTime;
                _balloon.transform.position = Vector3.MoveTowards(_balloon.transform.position, _balloon.transform.position + Vector3.up, _speed * 2 * Time.deltaTime);
                if (_timer <= 0)
                {
                    SetNextPoint();
                    _isFloating = false;
                }
            }
        }

        private Vector3 SetNextPoint()
        {
            _pointIndex++;
            if (_pointIndex % _points.Length == 0)
            {
                _pointIndex = 0;
            }
            return _points[_pointIndex].position;
        }

        public void GetHit()
        {
            _timer = _floatingTime;
            _isFloating = true;
        }
    }
}
