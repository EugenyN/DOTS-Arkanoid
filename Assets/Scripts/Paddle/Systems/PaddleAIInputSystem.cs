using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
public partial class PaddleAIInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithoutBurst()
            .ForEach((ref PaddleInputData inputData, in PlayerIndex playerIndex) =>
            {
                if (playerIndex.Value > 1)
                {
                    var state = (int) Time.ElapsedTime % (2 + playerIndex.Value);
                    inputData.Movement += state == 0 ? 1 : -1;
                }
            }).Run();
    }
}