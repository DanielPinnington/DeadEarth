using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    //public
    public void SetStateMachine(AIStateMachine stateMachine) { _stateMachine = stateMachine; }

    //Default Handlers
    public virtual void OnEnterState       () {}
    public virtual void OnExitState        () {}
    public virtual void OnAnimatorUpdated  () {}
    public virtual void OnAnimatorIKUpdated() {}
    public virtual void OnTriggerEvent     (AITriggerEventType eventType, Collider other) {}
    public virtual void OnDestinationReached(bool isReached) {}

    //Abstract Methods
    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    //Protected fields
    protected AIStateMachine _stateMachine;
}
