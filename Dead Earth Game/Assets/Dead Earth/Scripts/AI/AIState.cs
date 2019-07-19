using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------
// Class	:	AIState
// Desc		:	The base class of all AI States used by our AI System.
// ----------------------------------------------------------------------
public abstract class AIState : MonoBehaviour
{
    // Public Method
    // Called by the parent state machine to assign its reference
    public virtual void SetStateMachine(AIStateMachine stateMachine) { _stateMachine = stateMachine; }

    // Default Handlers
    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }

    // Abtract Methods
    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    // Protected Fields
    protected AIStateMachine _stateMachine;

    // ------------------------------------------------------------------
    // Name	:	OnAnimatorUpdated
    // Desc	:	Called by the parent state machine to allow root motion
    //			processing
    // ------------------------------------------------------------------
    public virtual void OnAnimatorUpdated()
    {

        // Get the number of meters the root motion has updated for this update and
        // divide by deltaTime to get meters per second. We then assign this to
        // the nav agent's velocity.
        if (_stateMachine.useRootPosition)
            _stateMachine.navAgent.velocity = _stateMachine.animator.deltaPosition / Time.deltaTime;

        // Grab the root rotation from the animator and assign as our transform's rotation.
        if (_stateMachine.useRootRotation)
            _stateMachine.transform.rotation = _stateMachine.animator.rootRotation;

    }

    // -----------------------------------------------------------------------------
    // Name	:	ConvertSphereColliderToWorldSpace
    // Desc	:	Converts the passed sphere collider's position and radius into
    //			world space taking into acount hierarchical scaling.
    // -----------------------------------------------------------------------------
    public static void ConvertSphereColliderToWorldSpace(SphereCollider col, out Vector3 pos, out float radius)
    {
        // Default Values
        pos = Vector3.zero;
        radius = 0.0f;

        // If no valid sphere collider return
        if (col == null)
            return;

        // Calculate world space position of sphere center
        pos = col.transform.position;
        pos.x += col.center.x * col.transform.lossyScale.x;
        pos.y += col.center.y * col.transform.lossyScale.y;
        pos.z += col.center.z * col.transform.lossyScale.z;

        // Calculate world space radius of sphere
        radius = Mathf.Max(col.radius * col.transform.lossyScale.x,
                            col.radius * col.transform.lossyScale.y);

        radius = Mathf.Max(radius, col.radius * col.transform.lossyScale.z);
    }

    // -----------------------------------------------------------------------
    // Name	:	FindSignedAngle
    // Desc	:	Returns the signed angle between to vectors (in degrees)
    // -----------------------------------------------------------------------
    public static float FindSignedAngle(Vector3 fromVector, Vector3 toVector)
    {
        if (fromVector == toVector)
            return 0.0f;

        float angle = Vector3.Angle(fromVector, toVector);
        Vector3 cross = Vector3.Cross(fromVector, toVector);
        angle *= Mathf.Sign(cross.y);
        return angle;
    }
}
