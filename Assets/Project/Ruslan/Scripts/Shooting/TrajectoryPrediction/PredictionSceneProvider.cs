using UnityEngine;
using UnityEngine.SceneManagement;


public class PredictionSceneProvider
{
    private Scene _scene;
    private PhysicsScene _physicsScene;

    public PhysicsScene PhysicsScene => _physicsScene;
    public Scene Scene => _scene;

    public void EnsureSceneCreated()
    {
        if (_scene.IsValid()) return;

        // Создаём отдельную сцену с физикой 3D
        _scene = SceneManager.CreateScene("PredictionScene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        _physicsScene = _scene.GetPhysicsScene();

        // Включаем ручную симуляцию Physics
        Physics.simulationMode = SimulationMode.Script;
        Physics.Simulate(Time.fixedDeltaTime);
    }
}
