﻿using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // OO objects
    private Cube ally;
    private List<Cube> allies;
    private Cube enemy;
    private List<Cube> enemies;
    private List<Cube> allCubes;

    private List<TextMeshPro> listOfDamageNumbers;
    private readonly float DELETE_THRESHOLD = 0.01f;

    private float sinceLastUpdate = 0f;

    // Unity objects
    public Transform allyTransform;
    public Transform enemyTransform;

    public Slider allyHealthSlider;
    public Slider enemyHealthSlider;

    public Slider enemySpeedSlider;

    public TextMeshPro damageNumbers;

    // Start is called before the first frame update
    void Start()
    {
        ally = new Cube
        {
            Health = 100,
            MaxHealth = 100,
            AttackMin = 6,
            AttackMax = 16,
            Defense = 5,
            TurnValue = 0,
            MaxTurnValue = 100,
            Speed = 100,
            CritRate = 25,
            CritDamage = 1.5,
            Name = "ally"
        };


        enemy = new Cube
        {
            Health = 100,
            MaxHealth = 100,
            AttackMin = 6,
            AttackMax = 16,
            Defense = 5,
            TurnValue = 0,
            MaxTurnValue = 100,
            Speed = 50,
            CritRate = 25,
            CritDamage = 1.5,
            Name = "enemy"
        };

        allies = new List<Cube>
        {
            ally
        };

        enemies = new List<Cube>
        {
            enemy
        };

        allCubes = new List<Cube> 
        { 
            ally, enemy 
        };

        listOfDamageNumbers = new List<TextMeshPro>();
    }

    private List<Cube> UpdateSpeeds()
    {
        List<Cube> cubesToMove = new List<Cube>();

        allCubes.ForEach(cube => {
            cube.TurnValue += cube.Speed;
            if (cube.TurnValue >= cube.MaxTurnValue)
            {
                cube.TurnValue -= cube.MaxTurnValue;
                cubesToMove.Add(cube);
            }
        });

        return cubesToMove;
        
    }

    private void CleanUpDamageNumbers()
    {
        List<TextMeshPro> toDelete = listOfDamageNumbers.Where(dmgNum => dmgNum.color.a <= DELETE_THRESHOLD).ToList();
        listOfDamageNumbers = listOfDamageNumbers.Where(dmgNum => dmgNum.color.a > DELETE_THRESHOLD).ToList();
        toDelete.ForEach(x => Destroy(x));
    }

    private void UpdatePositionsForDamageNumbers()
    {
        listOfDamageNumbers.ForEach(dmgNum => {
            dmgNum.color = new Color(dmgNum.color.r, dmgNum.color.g, dmgNum.color.b, dmgNum.color.a - (0.5f * Time.deltaTime));
            dmgNum.transform.position += new Vector3(0, 2f) * Time.deltaTime;
        });
    }
    private Cube PickRandomEnemy(Cube cube)
    {
        if (cube == ally)
        {
            return enemy;
        } 
        else
        {
            return ally;
        }
    }

    private void ModifyWithCritAsNecessary(TextMeshPro created, AttackResult result)
    {
        if (result.isCrit)
        {
            created.color = Color.red;
        }
    }

    private TextMeshPro CreateDamagePopup(Cube attackedCube, AttackResult result)
    {
        Transform attackedCubeTransform;
        if (attackedCube == ally)
        {
            attackedCubeTransform = allyTransform;
        }
        else
        {
            attackedCubeTransform = enemyTransform;
        }

        TextMeshPro created = Instantiate(damageNumbers, attackedCubeTransform.position + 2 * Vector3.up, Quaternion.Euler(0, -90, 0));
        created.SetText(result.damageApplied.ToString());
        listOfDamageNumbers.Add(created);

        return created;
    }

    private void UpdateSlider(Slider ofAttackedCube, Cube attackedCube)
    {
        ofAttackedCube.value = 1.0f * attackedCube.Health / attackedCube.MaxHealth;
    }

    private Slider GetCorrespondingSlider(Cube attackedCube)
    {
        if (attackedCube == ally)
        {
            return allyHealthSlider;
        }
        else
        {
            return enemyHealthSlider;
        }
    }

    // Update is called once per frame
    void Update() {
        CleanUpDamageNumbers();
        UpdatePositionsForDamageNumbers();

        // barrier for updating actions every second
        sinceLastUpdate += Time.deltaTime;
        if (sinceLastUpdate < 1.0f) return;
        sinceLastUpdate = 0f;

        List<Cube> cubesToMove = UpdateSpeeds();
        cubesToMove.ForEach(cube =>
        {
            Cube randEnemy = PickRandomEnemy(cube);
            AttackResult result = cube.Attack(randEnemy);
            Slider enemySlider = GetCorrespondingSlider(randEnemy);
            UpdateSlider(enemySlider, randEnemy);
            TextMeshPro created = CreateDamagePopup(randEnemy, result);
            ModifyWithCritAsNecessary(created, result);

            if (result.isEnemyDead)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        });
    }
}
