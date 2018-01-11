namespace Nez.AI.UtilityAI.Actions
{
	/// <summary>
	///     Appraisal for use with an ActionWithOptions
	/// </summary>
	public interface IActionOptionAppraisal<T, TU>
    {
        float GetScore(T context, TU option);
    }
}