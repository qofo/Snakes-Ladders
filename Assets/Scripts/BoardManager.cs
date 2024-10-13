using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
public class BoardManager : MonoBehaviour{
    [Serializable] //����ȭ
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    public int row = 10;
    public int column = 10;
    //8*8

    public GameObject[] floorTiles;
    public GameObject floorCountText;

    private Transform canvas;
    private GameObject player;
    public GameObject[] floors;

    private Transform floorTexts;
    private Transform boardHolder;//���� ������, ��� ������Ʈ�� �θ�
    private List <Vector3> gridPositions =  new List<Vector3>(); //������Ʈ  ��ġ ����

    //gridPositon �ʱ�ȭ
    void InitializeList()
    {
        gridPositions.Clear();
        for(int x=0; x<column; x++){
            for(int y=0; y<row; y++){
                gridPositions.Add(new Vector3(x,y,0f));
            }
        }
    }

    //Floor, outerwall ���� 
    void BoardSetup()
    {
        boardHolder = new GameObject ("Board").transform;                                       // ��� ��� Object�� �θ�� �� ������Ʈ
        floorTexts = new GameObject("FloorTexts").transform;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>().transform;
        floorTexts.transform.SetParent(canvas);
        player = GameObject.Find("Player");

        floors = new GameObject[row * column + 1];

        // Floor �����ϱ�
        for (int x = 0; x < column; x++)
        {
            for (int y = 0; y < row; y++)
            {
                GameObject toInstantiate = floorTiles[(x%2 + y%2)%2];
                //if (x == -1 || x == column || y == -1 || y == row)
                //  toInstantiate = outerWallTiles[Random.Range (0,outerWallTiles.Length)];
                createInstance(toInstantiate, new Vector3(x, y, 1f));
            }
        }

        // Player ���� ��ġ�� �̵���Ű��
        GameManager.instance.SetNextMove(1);
        Vector2 nextPos = GameManager.instance.GetNextMove(0).transform.position;
        player.transform.position = nextPos;

    }
    void createInstance(GameObject toInstantiate, Vector3 position)
    {
        GameObject instance = Instantiate(toInstantiate, position, Quaternion.identity);
        instance.transform.SetParent(boardHolder);

        Text tempText = Instantiate(floorCountText, position, Quaternion.identity).GetComponent<Text>();
        tempText.rectTransform.SetParent(floorTexts);

        tempText.transform.position = Camera.main.WorldToScreenPoint(position);
        int x = (int)position.x;
        int y = (int)position.y;
        int floorCount = column * (row - y - 1) ;
        if ((y % 2 == row%2))
            floorCount += column - x;
        else
            floorCount += x + 1;
        tempText.text = "" + floorCount;

        floors[floorCount] = instance;

    }
    // ���� ��ġ
    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    //gameMnager�� ȣ��
    public void SetupScene(int level){
        BoardSetup();
        InitializeList();
    }


}
