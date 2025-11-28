namespace SimulationEngine.Orchestration;

using SimulationEngine.Core;

/// <summary>
/// Coordinates multiple simulation models within a single run,
/// controlling time advancement and ensuring deterministic execution order.
/// </summary>
public interface ISimulationOrchestrator : IDisposable
{
    /// <summary>
    /// Unique identifier for this orchestration run.
    /// </summary>
    Guid RunId { get; }

    /// <summary>
    /// Current orchestrator state.
    /// </summary>
    OrchestratorState State { get; }

    /// <summary>
    /// All registered models in execution order.
    /// </summary>
    IReadOnlyList<IModelRegistration> Models { get; }

    /// <summary>
    /// Register a simulation model with the orchestrator.
    /// </summary>
    IModelRegistration Register(ISimulation model, ModelOptions? options = null);

    /// <summary>
    /// Register a simulation model that depends on other models.
    /// The model will execute after all its dependencies.
    /// </summary>
    IModelRegistration Register(ISimulation model, ModelOptions options, params string[] dependsOn);

    /// <summary>
    /// Initialize all registered models and prepare for execution.
    /// Models are initialized in dependency order.
    /// </summary>
    void Initialize(ISimulationContext context);

    /// <summary>
    /// Execute one time step across all models.
    /// Models execute in dependency order with barrier synchronization.
    /// </summary>
    OrchestratorStepResult Step();

    /// <summary>
    /// Check if all models have completed.
    /// </summary>
    bool IsComplete { get; }

    /// <summary>
    /// Event raised before each model executes its step.
    /// </summary>
    event EventHandler<ModelStepEventArgs>? BeforeModelStep;

    /// <summary>
    /// Event raised after each model completes its step.
    /// </summary>
    event EventHandler<ModelStepEventArgs>? AfterModelStep;

    /// <summary>
    /// Event raised after all models complete a time step (barrier reached).
    /// </summary>
    event EventHandler<BarrierEventArgs>? BarrierReached;
}

/// <summary>
/// Registration information for a model in the orchestrator.
/// </summary>
public interface IModelRegistration
{
    /// <summary>
    /// Unique identifier for this model registration.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The simulation model instance.
    /// </summary>
    ISimulation Model { get; }

    /// <summary>
    /// Configuration options for this model.
    /// </summary>
    ModelOptions Options { get; }

    /// <summary>
    /// IDs of models this model depends on.
    /// </summary>
    IReadOnlyList<string> Dependencies { get; }

    /// <summary>
    /// Current state of this model.
    /// </summary>
    ModelState State { get; }

    /// <summary>
    /// Number of steps this model has executed.
    /// </summary>
    long StepCount { get; }
}

/// <summary>
/// Configuration options for a registered model.
/// </summary>
public class ModelOptions
{
    /// <summary>
    /// Unique identifier for the model. If not provided, uses ISimulation.Name.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Execution priority (lower = earlier). Models with same priority
    /// execute in registration order. Dependencies override priority.
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// If true, model continues to step even after IsComplete.
    /// Useful for monitoring/logging models.
    /// </summary>
    public bool ContinueAfterComplete { get; set; }

    /// <summary>
    /// If true, orchestrator continues even if this model errors.
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Custom parameters passed to this model's context.
    /// </summary>
    public SimulationParameters? Parameters { get; set; }
}

/// <summary>
/// State of the orchestrator.
/// </summary>
public enum OrchestratorState
{
    /// <summary>
    /// Created but not initialized.
    /// </summary>
    Created,

    /// <summary>
    /// Models registered, ready to initialize.
    /// </summary>
    Ready,

    /// <summary>
    /// Initialized and running.
    /// </summary>
    Running,

    /// <summary>
    /// Paused mid-execution.
    /// </summary>
    Paused,

    /// <summary>
    /// All models completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Stopped due to error.
    /// </summary>
    Error
}

/// <summary>
/// State of an individual model.
/// </summary>
public enum ModelState
{
    Registered,
    Initializing,
    Ready,
    Stepping,
    Completed,
    Error
}

/// <summary>
/// Result of an orchestrator step.
/// </summary>
public enum OrchestratorStepResult
{
    /// <summary>
    /// All models stepped successfully, continue execution.
    /// </summary>
    Continue,

    /// <summary>
    /// All models have completed.
    /// </summary>
    Completed,

    /// <summary>
    /// Execution paused (by request or breakpoint).
    /// </summary>
    Paused,

    /// <summary>
    /// A required model encountered an error.
    /// </summary>
    Error
}

/// <summary>
/// Event arguments for model step events.
/// </summary>
public class ModelStepEventArgs : EventArgs
{
    public required IModelRegistration Model { get; init; }
    public required long StepNumber { get; init; }
    public SimulationStepResult? Result { get; init; }
    public Exception? Error { get; init; }
}

/// <summary>
/// Event arguments for barrier synchronization events.
/// </summary>
public class BarrierEventArgs : EventArgs
{
    public required long StepNumber { get; init; }
    public required IReadOnlyList<IModelRegistration> Models { get; init; }
    public required TimeSpan SimulatedTime { get; init; }
}
