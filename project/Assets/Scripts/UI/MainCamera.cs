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
    private const float XRadius = 5f;
    private const float YRadius = 8f;
    private const float AngleYAxis = 45f; // 初期状態では真後ろから
    private const float AngleXAxis = 270f; // 初期状態では斜め下45度からの角度で見る
    private const float CameraAngleSensitivity = 3f;

    [SerializeField] private TargetPanel _cursorObject = default!;
    [SerializeField] private InputReceiver _inputReceiver = default!;

    private void Start()
    {
        _distanceParam = new DistanceParam()
        {
            DistanceCamSq = Mathf.Sqrt((XRadius * XRadius) + (YRadius * YRadius)), // カメラから注視する物体までの距離
            XAngleAxis = AngleXAxis,
            YAngleAxis = AngleYAxis
        };
    }

    private void Update()
    {
        if (_cursorObject == null || _inputReceiver == null)
        {
            Debug.Log("_cursorObject or _inputReceiver is null");
            return;
        }

        FollowCursor();
    }

    private void LateUpdate()
    {
        if (_cursorObject == null || _inputReceiver == null)
        {
            Debug.Log("_cursorObject or _inputReceiver is null");
            return;
        }

        /* カメラ用パラメータの更新 */
        var distanceParamDiff = _inputReceiver.GetInputMouseParam();
        /* マウスホイールで注視オブジェクトとの距離更新 */
        _distanceParam.DistanceCamSq -= distanceParamDiff.distanceCamSqDiff;
        /* 右クリックを押したままマウスを動かした場合に視点移動 */
        _distanceParam.XAngleAxis -= distanceParamDiff.angleAxisDiff.x * CameraAngleSensitivity; /* x軸方向の移動量 */
        _distanceParam.YAngleAxis -= distanceParamDiff.angleAxisDiff.y * CameraAngleSensitivity; /* y軸方向の移動量 */
    }

    private void FollowCursor()
    {
        float yRad = _distanceParam.YAngleAxis * Mathf.Deg2Rad;
        float xRad = _distanceParam.XAngleAxis * Mathf.Deg2Rad;

        var position = _cursorObject.transform.position;
        transform.position = new Vector3(
            position.x + (_distanceParam.DistanceCamSq * Mathf.Sin(yRad) * Mathf.Cos(xRad)),
            position.y + (_distanceParam.DistanceCamSq * Mathf.Cos(yRad)),
            position.z + (_distanceParam.DistanceCamSq * Mathf.Sin(yRad) * Mathf.Sin(xRad))
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
