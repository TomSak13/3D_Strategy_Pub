using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public struct DistanceParam
    {
        public float DistanceCamSq;
        public float XAngleAxis;
        public float YAngleAxis;
    }

    private DistanceParam _distanceParam;
    private const float _xRadius = 5f;
    private const float _yRadius = 8f;
    private const float _angle_yAxis = 45f; // 初期状態では真後ろから
    private const float _angle_xAxis = 270f; // 初期状態では斜め下45度からの角度で見る
    private const float _cameraAngleSensitivity = 3f;

    [SerializeField] private TargetPanel _cursorObject;
    [SerializeField] private InputReceiver _inputReceiver;

    private void Start()
    {
        _distanceParam = new DistanceParam()
        {
            DistanceCamSq = Mathf.Sqrt((_xRadius * _xRadius) + (_yRadius * _yRadius)), // カメラから注視する物体までの距離
            XAngleAxis = _angle_xAxis,
            YAngleAxis = _angle_yAxis
        };
    }

    private void Update()
    {
        if (_cursorObject == null || _inputReceiver == null)
        {
            Debug.Log("_cursorObject or _inputReceiver is null");
            return;
        }

        FollowCusor();
    }

    private void LateUpdate()
    {
        if (_cursorObject == null || _inputReceiver == null)
        {
            Debug.Log("_cursorObject or _inputReceiver is null");
            return;
        }

        /* カメラ用パラメータの更新 */
        _inputReceiver.UpdateLatestCameraDistance(ref _distanceParam, _cameraAngleSensitivity);
    }
    
    private void FollowCusor()
    {
        float y_rad = _distanceParam.YAngleAxis * Mathf.Deg2Rad;
        float x_rad = _distanceParam.XAngleAxis * Mathf.Deg2Rad;

        transform.position = new Vector3(
            _cursorObject.transform.position.x + (_distanceParam.DistanceCamSq * Mathf.Sin(y_rad) * Mathf.Cos(x_rad)),
            _cursorObject.transform.position.y + (_distanceParam.DistanceCamSq * Mathf.Cos(y_rad)),
            _cursorObject.transform.position.z + (_distanceParam.DistanceCamSq * Mathf.Sin(y_rad) * Mathf.Sin(x_rad))
            );
        transform.LookAt(_cursorObject.transform);
    }

    public void MoveCursor(Unit unit)
    {
        if (_cursorObject == null)
        {
            return;
        }
        _cursorObject.MoveFocusCell(unit.OnCell);
    }
}
