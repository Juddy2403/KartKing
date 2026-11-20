using System;
using Managers;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    [RequireComponent(typeof(UIDocument))]
    public class HUDManager : MonoBehaviour {
        [SerializeField] private VisualTreeAsset scoreEntry;

        private UIDocument        hud;
        private Label             timerText;
        private ScoreBoardElement scoreBoardElement;

        private void Awake() {
            hud               = GetComponent<UIDocument>();
            timerText         = hud.rootVisualElement.Q<Label>("TimerText");
            scoreBoardElement = hud.rootVisualElement.Q<ScoreBoardElement>();

            TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.RoundTime);
            timerText.text = timeSpan.ToString(@"mm\:ss");
        }

        private void Start() {
            scoreBoardElement.Initialize(scoreEntry, ScoreManager.Instance.Scores, true);

            GameManager.Instance.onTimeTick.AddListener(OnTimeTick);
            ScoreManager.Instance.onScoreUpdate.AddListener(OnScoreUpdate);
        }

        private void OnScoreUpdate(ScoreData data) {
            scoreBoardElement.UpdateScore(data, true);
        }

        private void OnTimeTick(float time) {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            timerText.text = timeSpan.ToString(@"mm\:ss");
            if (GameManager.Instance.IsInLateGame) timerText.style.color = Color.red;
        }
    }
}