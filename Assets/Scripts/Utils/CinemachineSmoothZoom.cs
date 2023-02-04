using UnityEngine;
using Cinemachine;
 
[SaveDuringPlay]
[AddComponentMenu("")] // Hide in menu
[DocumentationSorting(DocumentationSortingAttribute.Level.UserRef)]
#if UNITY_2018_3_OR_NEWER
[ExecuteAlways]
#else
[ExecuteInEditMode]
#endif
public class CinemachineSmoothZoom : CinemachineExtension
{
    public float lerpSpeed;
    class VcamExtraState
    {
        public Vector3 m_previousFramePos;
        public float m_previousFrameSize;
    }
    protected override void PostPipelineStageCallback(
                CinemachineVirtualCameraBase vcam,
                CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        VcamExtraState extra = GetExtraState<VcamExtraState>(vcam);
        if (deltaTime < 0)
        {
            extra.m_previousFramePos = state.FinalPosition;
            extra.m_previousFrameSize = state.Lens.OrthographicSize;
        }
 
        if (stage == CinemachineCore.Stage.Body)
        {
            Vector3 targetPos = Vector3.Lerp(extra.m_previousFramePos, state.FinalPosition, lerpSpeed);
            Vector3 delta = state.FinalPosition - targetPos;
 
            state.PositionCorrection += -delta;
            extra.m_previousFramePos = targetPos;
 
            float targetSize = Mathf.Lerp(extra.m_previousFrameSize, state.Lens.OrthographicSize, lerpSpeed);
            LensSettings lens = state.Lens;
            lens.OrthographicSize = extra.m_previousFrameSize = targetSize;
            state.Lens = lens;
        }
    }
}