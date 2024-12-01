using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
using static UnityEditor.PlayerSettings;
public class BoardManager : MonoBehaviour{
    
    public int snake_num = 10;
    public int ladder_num = 10;
    [Serializable] //직렬화
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
    public GameObject bossTile;
    public GameObject shopTile;
    public GameObject floorCountText;
    public GameObject ladder;
    public GameObject snake;

    private Transform uiCanvas;
    private GameObject player;
    public GameObject[] floors;

    private Transform floorTexts;
    private Transform boardHolder;//계층 정리용, 모든 오브젝트의 부모
    private List <Vector3> gridPositions =  new List<Vector3>(); // 오브젝트  위치 저장
    private List<Vector3> usedPositions = new List<Vector3>(); // 뱀 또는 사다리가 있는 타일

    public List<Vector3> ladderStartPos = new List<Vector3>();
    public List<Vector3> ladderEndPos = new List<Vector3>();
    public List<Vector3> snakeStartPos = new List<Vector3>();
    public List<Vector3> snakeEndPos = new List<Vector3>();

    //gridPositon 초기화
    void InitializeList()
    {
        gridPositions.Clear();
        for(int x=0; x<column; x++){
            for(int y=0; y<row; y++){
                gridPositions.Add(new Vector3(x,y,0f));
            }
        }
    }

    //Floor, outerwall 생성 
    void BoardSetup()
    {
        InitializeList();

        boardHolder = new GameObject ("Board").transform;                                       // 모든 배경 Object의 부모로 둘 오브젝트
        floorTexts = new GameObject("FloorTexts").transform;
        uiCanvas = GameObject.Find("UICanvas").GetComponent<Canvas>().transform;
        floorTexts.transform.SetParent(uiCanvas);
        player = GameObject.Find("Player");

        floors = new GameObject[row * column + 1];

        // Floor 생성하기
        for (int x = 0; x < column; x++)
        {
            for (int y = 0; y < row; y++)
            {
                GameObject toInstantiate = floorTiles[(x%2 + y%2)%2];
                //if (x == -1 || x == column || y == -1 || y == row)
                //  toInstantiate = outerWallTiles[Random.Range (0,outerWallTiles.Length)];
                createFloor(toInstantiate, new Vector3(x, y, 1f));
            }
        }

        // Player 시작 위치로 이동시키기
        GameManager.instance.SetNextMove(1);
        Vector2 nextPos = GameManager.instance.GetNextMove(0).transform.position;
        player.transform.position = nextPos;
        gridPositions.RemoveAt(0);
        usedPositions.Add(nextPos);

        // 보스 타일 만들기
        nextPos = floors[column * row].transform.position;
        GameObject temp = floors[(column * row)];
        floors[(column * row)] = Instantiate(bossTile, nextPos, Quaternion.identity);
        Destroy(temp);
        usedPositions.Add(nextPos);

        nextPos = floors[column * row / 2].transform.position;
        temp = floors[column * row / 2];
        floors[column * row / 2] = Instantiate(bossTile, nextPos, Quaternion.identity);
        Destroy(temp);
        usedPositions[0] = nextPos;

        
        // 사다리 만들기
        Vector3 startPos;
        Vector3 endPos;
        int start_x, end_x, start_y, end_y;
        GameObject newObject;
        float direction, length;
        for (int i = 0; i < 4; i++)
        {
            // 사다리 시작 위치 생성(범위 y=0~13)
            startPos = FindRandomPos(0, column, 0, 14);
            if (startPos == Vector3.zero)
                continue;
            newObject = Instantiate(ladder, startPos, Quaternion.identity);

            // 사다리 도착 위치 생성(범위 x=pos.x-6 ~ pos.x+6, y = y+1 ~ y+4)
            start_x = (int)((startPos.x - 6 > 0) ? startPos.x - 6 : 0);
            end_x = (int)((startPos.x + 6 < column) ? startPos.x + 6 : column);
            start_y = (int)(startPos.y + 1);
            end_y = (int)((startPos.y + 5 < row) ? startPos.y + 5 : row);
            endPos = (FindRandomPos(start_x, end_x, start_y, end_y));

            //Debug.Log($"ladder({startPos.x}, {startPos.y}) ({endPos.x}, {endPos.y})");


            // 세로로 세워진(-90인) 스프라이트라 시작-끝 벡터에서 90도를 빼야 함
            direction = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg - 90;
            newObject.transform.Rotate(new Vector3(0f, 0f, direction));

            // 길이 조정하기
            length = (float)Math.Sqrt(Math.Pow(startPos.x - endPos.x, 2) +  Math.Pow(endPos.y - startPos.y, 2));
            newObject.transform.localScale = new Vector3(0.5f, 0.2f * length, 1);

            // 리스트에 추가하기
            ladderStartPos.Add(startPos);
            ladderEndPos.Add(endPos);
        }

        // 뱀 만들기
        for (int i = 0; i < snake_num; i++)
        {
            // 뱀 시작 위치 생성(범위 y=row-1 ~ row-17)
            startPos = FindRandomPos(0, column, row - 18, row-1);
            if (startPos == Vector3.zero)
                continue;
            newObject = Instantiate(snake, startPos, Quaternion.identity);

            // 뱀 도착 위치 생성(범위 x=pos.x-6 ~ pos.x+6, y = y-4 ~ y)
            start_x = (int)((startPos.x - 6 > 0) ? startPos.x - 6 : 0);
            end_x = (int)((startPos.x + 6 < column) ? startPos.x + 6 : column);
            start_y = (int)((startPos.y -4 > 0) ? startPos.y - 4 : 0);
            end_y = (int)(startPos.y);
            endPos = (FindRandomPos(start_x, end_x, start_y, end_y));

            //Debug.Log($"snake({startPos.x}, {startPos.y}) ({endPos.x}, {endPos.y})");


            // 세로로 세워진(-90인) 스프라이트라 시작-끝 벡터에서 90도를 더해야 함
            direction = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg + 90;
            newObject.transform.Rotate(new Vector3(0f, 0f, direction));

            // 길이 조정하기
            length = (float)Math.Sqrt(Math.Pow(startPos.x - endPos.x, 2) + Math.Pow(endPos.y - startPos.y, 2));
            newObject.transform.localScale = new Vector3(0.5f, 0.25f * length, 1);

            // 리스트에 추가하기
            snakeStartPos.Add(startPos);
            snakeEndPos.Add(endPos);
        }

        // 상점 타일 만들기
        for (int i = 0; i < 15; i++)
        {
            nextPos = FindRandomPos(0, column, 0, row);
            int x = (int)nextPos.x, y = (int)nextPos.y;
            int floorCount = column * y;
            if ((y % 2 == 1))
                floorCount += column - x;
            else
                floorCount += x + 1;
            temp = floors[floorCount];
            floors[floorCount] = Instantiate(shopTile, nextPos, Quaternion.identity);
            Destroy(temp);
        }

    }
    void createFloor(GameObject toInstantiate, Vector3 position)
    {
        GameObject instance = Instantiate(toInstantiate, position, Quaternion.identity);
        instance.transform.SetParent(boardHolder);
        instance.transform.localScale = new Vector3(0.90f, 0.90f, 0f);

        // 텍스트 오브젝트 만들기
        GameObject floorText = Instantiate(floorCountText, position, Quaternion.identity);
        Text tempText = floorText.GetComponent<Text>();
        tempText.rectTransform.SetParent(floorTexts);

        floorText.GetComponent<TextController>().SetPosition(position);
        tempText.transform.position = Camera.main.WorldToScreenPoint(position);
        int x = (int)position.x;
        int y = (int)position.y;
        int floorCount = column * y;
        if ((y % 2 == 1))
            floorCount += column - x;
        else
            floorCount += x + 1;
        tempText.text = "" + floorCount;

        floors[floorCount] = instance;

    }

    // 랜덤 위치
    Vector3 FindRandomPos(int x_start, int x_end, int y_start, int y_end)
    {
        Vector3 position = Vector3.zero;
        for (int i = 0; i < snake_num; i++)
        {
            position = new Vector3(Random.Range(x_start, x_end), Random.Range(y_start, y_end), 0f);
            if (!usedPositions.Contains(position))
                break;
        }

        usedPositions.Add(position);
        return position;
    }

    //gameMnager가 호출
    public void SetupScene(int level){
        BoardSetup();
        InitializeList();
    }


}
