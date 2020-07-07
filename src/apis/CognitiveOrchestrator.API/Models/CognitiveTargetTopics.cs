namespace CognitiveOrchestrator.API.Models
{
    /// <summary>
    /// Target topics are what the system can publish messages to indicating a new action is required.
    /// Currently the platform support only CamFrameAnalysis topic
    /// </summary>
    public enum CognitiveTargetTopics
    {
        CamFrameAnalysis
    }
}