using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class JuggleAgent : Agent
{

    public Rigidbody ball;
    private Rigidbody player;
    public float speed = 30.0f;
    private float curBallYDiff = 0.0f;
    private float lastBallYDiff = 0.0f;
    private float lastBallY = 5.0f;
    private bool collied = false;

    void Start()
    {
        player = this.GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ball.transform.localPosition);
        sensor.AddObservation(ball.velocity);
        sensor.AddObservation(ball.rotation);
        sensor.AddObservation(ball.angularVelocity);

        sensor.AddObservation(player.transform.localPosition);
        sensor.AddObservation(player.velocity);
        sensor.AddObservation(player.rotation);
        sensor.AddObservation(player.angularVelocity);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        if(player.transform.localPosition.y == 1.0f){
            controlSignal.y = actions.ContinuousActions[2] * 10.0f;
        }

        player.AddForce(controlSignal * speed);

        curBallYDiff = ball.transform.localPosition.y - lastBallY;
        if(curBallYDiff > 0.0f && lastBallYDiff < 0.0f && collied){
            AddReward(0.1f);
        }

        collied = false;
        lastBallYDiff = curBallYDiff;
        lastBallY = ball.transform.localPosition.y;

        if(ball.transform.localPosition.y < 1.5f || 
            Mathf.Abs(player.transform.localPosition.x)  > 10.0f ||
            Mathf.Abs(player.transform.localPosition.z)  > 10.0f ){
                EndEpisode();
            }
    }

    public override void OnEpisodeBegin()
    {
        ball.transform.localPosition = new Vector3(Random.value * 10 - 5, 5.0f, Random.value * 10 - 5);
        ball.velocity = Vector3.zero;
        ball.rotation = Quaternion.Euler(Vector3.zero);
        ball.angularVelocity = Vector3.zero;

        player.transform.localPosition = Vector3.up;
        player.velocity = Vector3.zero;
        player.rotation = Quaternion.Euler(Vector3.zero);
        player.angularVelocity = Vector3.zero;

        curBallYDiff = 0.0f;
        lastBallYDiff = 0.0f;
        lastBallY = 0.0f;
        collied = false;
        base.OnEpisodeBegin();
    }

    public override void Heuristic(in ActionBuffers actionsOut){
        ActionSegment<float> varcontinuousActionsOut = actionsOut.ContinuousActions;
        varcontinuousActionsOut[0] = Input.GetAxis("Horizontal");
        varcontinuousActionsOut[1] = Input.GetAxis("Vertical");
        varcontinuousActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
    }

    private void OnCollisionEnter(Collision collision){
        if(collision.rigidbody == ball){
            collied = true;
        }
    }

}
