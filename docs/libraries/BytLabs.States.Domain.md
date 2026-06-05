# BytLabs.States.Domain

Domain building blocks for **state-machine aggregates**: model an aggregate whose lifecycle is a set
of states with rule-guarded transitions driven by triggers. Rules are evaluated with
[RulesEngine](https://github.com/microsoft/RulesEngine) expressions. This is an advanced/optional
package — use it only when an aggregate genuinely has a formal state machine.

## Install

```xml
<PackageReference Include="BytLabs.States.Domain" />
```

## What's inside

| Type | Purpose |
|------|---------|
| `StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>` | Base for an aggregate that has a current `StateId` and belongs to a state machine (`StateMachineId`) |
| `…​.StateBase` | A state entity |
| `…​.TransitionBase` | A transition: `From?`, `To`, optional `Trigger`, and a set of `TransitionRule`s |
| `…​.StateMachineAggregateBase` | Owns `States` + `Transitions`; exposes `Fire(trigger, entity)` and `Goto(state, entity)` |
| `Trigger` | Value object naming a trigger event |
| `TransitionRule` | A named RulesEngine expression evaluated against the entity (`Evaluate(...)`) |

## How it works

- A **state machine** (`StateMachineAggregateBase`) holds the valid `States` and the `Transitions`
  between them. Its constructor validates that every state referenced by a transition exists.
- A **transition** has an optional `From` (null = "from anywhere"), a `To`, an optional `Trigger`,
  and zero or more `TransitionRule`s (guards).
- **`Fire(trigger, entity)`** applies transitions whose trigger matches and whose `From` matches the
  entity's current `StateId`; each matching transition's rules must pass (else `DomainException`),
  then `entity.StateId` becomes `To`.
- **`Goto(toState, entity)`** performs a triggerless transition to a target state, evaluating rules.
- Rules are RulesEngine expressions evaluated with the entity bound as the `entity` parameter; a
  failing rule throws `DomainException("Failing at rule(s): ...")`.

## Usage sketch

```csharp
// Define concrete State, Transition, StateMachine, and the stateful aggregate by closing the generics.
// Then drive state changes through the state machine:

stateMachine.Fire(new Trigger("Submit"), order);   // trigger-based transition (rules guarded)
stateMachine.Goto(OrderState.Shipped, order);       // direct transition (rules guarded)
```

> Because the base type closes seven generic parameters, define a small set of concrete types per
> aggregate (state, transition, state machine, stateful aggregate) and keep transition rules as named
> RulesEngine expressions. Persisted with the same MongoDB repository pattern as other aggregates.

## Related packages
- [BytLabs.Domain](BytLabs.Domain.md) — `AggregateRootBase`, `Entity`, `ValueObject`, `DomainException` it builds on.
