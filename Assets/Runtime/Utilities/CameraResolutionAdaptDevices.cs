using UnityEngine;

public class CameraResolutionAdaptDevices : MonoBehaviour
{
    public enum FixedCoordinate
    {
        NotSet, Width, Height
    }

    [SerializeField] private FixedCoordinate _fixedCoordinate;
    [SerializeField] private Vector2Int _defaultScreenSize = new Vector2Int(1080, 2160);

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();

        if (_fixedCoordinate == FixedCoordinate.Width)
        {
            float oldAspect = _defaultScreenSize.x * 1.0f / _defaultScreenSize.y;

            if (_cam.orthographic)
                _cam.orthographicSize *= oldAspect / _cam.aspect;
            else
                //_cam.fieldOfView = 2 * Mathf.Atan(Mathf.Tan(_cam.fieldOfView / 2) * oldAspect / _cam.aspect);
                _cam.transform.position += oldAspect / _cam.aspect * Vector3.forward;
        }
    }
}
