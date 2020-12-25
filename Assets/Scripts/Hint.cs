﻿using UnityEngine;
using System.Collections;

public class Hint : MonoBehaviour
{

    public Sprite hintImage1, hintImage2, hintImage3;
    [HideInInspector]
    public bool show = false;
    [HideInInspector]
    public GameObject[] hintPath;
    private Transform[] hints;
    private float timeStep = 0.1f;
    private float timer;
    private int counter = 1, numHints;
    private static Hint instance;
    private Color hintColor;
    private bool used;

    public void ShowHint()
    {
        if (Prefs.hintCount > 0 || used)
        {
            SoundManager.instance.PlaySound(SoundManager.instance.hintSound);
            instance.show = true;
            if (!used)
            {
                Prefs.hintCount -= 1;
                used = true;
            }
            GameManager.instance.SetHintCountText();
        }
        else
        {
            GameManager.instance.ShowEarnHintsDialog();
        }
    }

    public void Reset()
    {
        for (int i = 0; i < hints.Length; i++)
            if (hints[i].gameObject.GetComponent<SpriteRenderer>() != null)
                hints[i].gameObject.GetComponent<SpriteRenderer>().color = Color.clear;
        counter = 1;
        show = false;
        timer = 0;
    }

    void Awake()
    {
        instance = this;
    }

    void ShowHintPath()
    {
        timer += Time.deltaTime;
        if (timer > timeStep)
        {
            hints[counter].gameObject.GetComponent<SpriteRenderer>().color = hintColor;
            timer = 0;
            show = false;
        }
        if (timer == 0 && counter < numHints)
        {
            counter += 1;
            show = true;
        }
    }

    void Start()
    {
        show = false;
        hints = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < hints.Length; i++)
            if (hints[i].gameObject.GetComponent<SpriteRenderer>() != null)
                hints[i].gameObject.GetComponent<SpriteRenderer>().color = Color.clear;
        numHints = hints.Length - 1;
        hintColor = new Color(1f, 1f, 1f, 0.65f);
    }

    void Update()
    {
        if (show)
            ShowHintPath();
        if (Input.GetMouseButtonDown(0))
        {
            if (counter == numHints)
                Reset();
        }
    }
}
