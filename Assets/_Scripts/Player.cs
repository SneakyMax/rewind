﻿using System;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

namespace Assets._Scripts
{
    [UnityComponent, RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
		[EventRef]
		public string SoundWalkEventName = "event:/footsteps_carpet";
        private EventInstance walkSound;

        [EventRef]
		public string SoundDoorOpenEventName = "event:/Door_Open";


		public static Player Instance { get; private set; }

        [AssignedInUnity]
        public Transform Center;

        [AssignedInUnity]
        public Transform Rotation;

        [AssignedInUnity, Range(0, 10)]
        public float MoveSpeed = 5;

        private Vector2 desiredMovement;
        private new Rigidbody2D rigidbody;

        [UnityMessage]
        public void Awake()
        {
            Instance = this;
            rigidbody = GetComponent<Rigidbody2D>();

            walkSound = RuntimeManager.CreateInstance(SoundWalkEventName);
        }

        [UnityMessage]
        public void Update()
        {
            var rawMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (rawMovement.sqrMagnitude > 1)
                rawMovement.Normalize();

            desiredMovement = rawMovement * MoveSpeed;

            if (rigidbody.velocity.sqrMagnitude > 0)
                rigidbody.velocity = new Vector2();

            if(desiredMovement.IsZero() == false)
                Rotation.transform.rotation = Quaternion.AngleAxis(new Vector3().DirectionToDegrees(desiredMovement), Vector3.forward);
        }

        [UnityMessage]
        public void FixedUpdate()
        {
			if (desiredMovement.IsZero ()) 
			{
				walkSound.stop (STOP_MODE.ALLOWFADEOUT);
				return;
			}

			PLAYBACK_STATE walkSoundState;
			walkSound.getPlaybackState(out walkSoundState);
			if (walkSoundState != PLAYBACK_STATE.PLAYING)
				walkSound.start();

            var fixedMovement = desiredMovement * Time.fixedDeltaTime;
            var newPosition = transform.position + (Vector3)fixedMovement;

            rigidbody.MovePosition(newPosition);
        }

        [UnityMessage]
        public void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Transition"))
            {
				RuntimeManager.PlayOneShot(SoundDoorOpenEventName, transform.position);
				EnterTransition(collider.gameObject.GetComponentInParent<MapTransition>());
            }
        }

        private void EnterTransition(MapTransition transitionObject)
        {
            if (transitionObject == null)
                throw new InvalidOperationException("Missing transition object.");

            MapController.Instance.ChangeMap(transitionObject.ToMap);

            var destination = MapController.Instance.GetTransitionDestination(transitionObject.ToName);

            if (destination == null)
                throw new InvalidOperationException("Couldn't find transition point " + transitionObject.ToName);

            transform.position = destination.transform.position;
        }
    }
}
