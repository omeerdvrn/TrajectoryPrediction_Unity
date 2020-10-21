using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPrediction : MonoBehaviour
{
    #region Singleton
private static TrajectoryPrediction _instance;
private void Awake()
{
    if (_instance == null)
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }
    else
    {
        Destroy(this);
    }
}
public static TrajectoryPrediction GetInstance()
{
    return _instance;
}
#endregion

    private Scene _mainScene;//Created a scene to hold our main scene.
    private Scene _predictionScene;//Also created a scene to hold our prediction scene.
    private PhysicsScene _mainPhysicsScene;//Created a physics scene to hold our main physics.
    private GameObject _predictionBall;//Created a gameobject to hold  instantiated moving object in the prediction scene.
    private PhysicsScene _predictionPhysicsScene;//Also created a physics scene to hold our prediction physics.
    [SerializeField] private GameObject _objPrefab;//Prefab of the instantiated moving object in the prediction scene.
    [SerializeField] private Transform _objTransform;//Transfrom of the instantiate point.
    public LineRenderer _lineRenderer;//Line renderer to render predictions.
    public int _linePositionAmount;//Amount of the line positions.

    private void Start()
    {
        Physics.autoSimulation = false;//We dont want to simulate physics automatically.
        _mainScene = SceneManager.GetActiveScene();//Set our scene to _mainScene.
        _mainPhysicsScene = _mainScene.GetPhysicsScene();//Set our main scene's physics to _mainScenePhysics.
        CreateSceneParameters sceneParam = new CreateSceneParameters(LocalPhysicsMode.Physics3D);//Created 3D Scene Parameters.
        _predictionScene = SceneManager.CreateScene("ScenePredictionPhysics", sceneParam);//Created 3D scene called "ScenePredictionPhysics" that has 3D parameters and set it to _predictionScene.
        _predictionPhysicsScene = _predictionScene.GetPhysicsScene();//Set our prediction scene's physics to _predictionPhysicsScene.
        _lineRenderer.positionCount = _linePositionAmount;//Set our position count.
        _lineRenderer.enabled = false;//Disabled our line renderer.
    }

    
    private void FixedUpdate()
    {
        if (!_mainPhysicsScene.IsValid())//Check if our scene has valid physics.
            return;
        _mainPhysicsScene.Simulate(Time.fixedDeltaTime);//Simulated the main scene.
    }
    
    //Predict: predicts the path of the ball and renders lines.
    public void Predict(Vector3 force)
    {
        _lineRenderer.enabled = true;//Enabled our line renderer.
        if (!_mainPhysicsScene.IsValid() || !_predictionPhysicsScene.IsValid())//Check if our both scenes has valid physics. 
            return;
        _predictionBall = Instantiate(_objPrefab,_objTransform.position,Quaternion.identity);//Instantiated one of our prediction ball.
        SceneManager.MoveGameObjectToScene(_predictionBall,_predictionScene);//Set our prediction ball to prediction scene.
        _predictionBall.GetComponent<Rigidbody>().AddForce(force);//Added force to prediction ball.
        
        for (int i = 0; i < _linePositionAmount; i++)//Loop that turns _linePositionAmount times.
        {
            _predictionPhysicsScene.Simulate(Time.fixedDeltaTime);//Simulate our prediction physics.
            _lineRenderer.SetPosition(i,_predictionBall.transform.position);//Rendered a line at prediction ball's position.
        }

        Destroy(_predictionBall);//Destroyed our prediction ball, we don't need it.
    }

    //Release: really moves the ball.
    public void Release(Vector3 force)
    {
        if (!_mainPhysicsScene.IsValid() || !_predictionPhysicsScene.IsValid())//Check if our both scenes has valid physics. 
            return;
        GameObject ball = Instantiate(_objPrefab,_objTransform.position,Quaternion.identity);//Instantiated our real ball.
        ball.tag = "ball";//Set tag to ball.
        ball.GetComponent<Rigidbody>().AddForce(force);//Added force to our real ball.
    }
    
    
}


