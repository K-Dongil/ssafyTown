using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public interface IMsgHandler //�������� �����͸� �޴� ����� ���� �������̽�
{
    public void HandleMsg(string payload); //�������� �޽����� ���� ����Ǵ� �Լ�
}

public class SocketClient : MonoBehaviour
{
    public string url = "ws://localhost"; //���� url
    public int port = 31012; //���� ��Ʈ

    private WebSocket webSocket; //�� ���� �ν��Ͻ�

    private Dictionary<string, IMsgHandler> handlerDic; //�����͵��� �޾� ó�����ִ� handler�� ��ųʸ�

    private Queue<DataVO> packetList = new Queue<DataVO>(); //unity ���� �������̱� ������ �����Ͱ� ���� ��Ƶΰ�, update���� ����Ǿ� �ϱ� ������ ��Ƶα� ���� ť

    [SerializeField]
    private Transform handlerParent; //handler���� �޾Ƶ� ������Ʈ

    //������ �̱���
    private static SocketClient _instance = null;
    public static SocketClient Instance
    {
        get => _instance;
        set => _instance = value;
    }

    private void Awake()
    {
        Instance = this;
        handlerDic = new Dictionary<string, IMsgHandler>(); //��ųʸ� �ʱ�ȭ
    }

    private void Start()
    {
        IMsgHandler[] handlerList = handlerParent.GetComponents<IMsgHandler>(); //�ڵ鷯���� ��������

        for (int i = 0; i < handlerList.Length; i++)
        {
            handlerDic.Add(GetTypeString(handlerList[i].GetType().ToString()), handlerList[i]); //�ش� Ÿ�Կ� �ش��ϴ� ��ũ��Ʈ�� �־��ش�
        }

        ConnectSocket(url, port);
    }

    //~~Handler�� �����̱⶧���� type�� �������� ���� �Լ�, �ؿ����� ���÷� Transform�� ����ϰڽ��ϴ� (ex. TransformHandler
    public string GetTypeString(string s)
    {
        List<int> idx = new List<int>();
        s = s.Replace("Handler", ""); //�ڿ����� handler�� ���ݴϴ�. �׷��� Transform�� �����ϴ�.

        for (int i = 1; i < s.Length; i++) //0��°�� ������ �빮�ڴ� �˻縦 ������ �ʽ��ϴ�.
        {
            if (s[i].Equals(char.ToUpper(s[i]))) //���� 0��°���� �빮�ڰ� ������ List�� �ش� idx�� �߰����ݴϴ�.
            {
                idx.Add(i);
            }
        }

        for (int i = 0; i < idx.Count; i++) //���� �빮�ڰ� �־��ٸ� for���� ���ƿ�.
        {
            if (i >= 1) //�׷��� 2���̻� ������ �ƴٸ�
            {
                s = s.Insert(idx[i] + i, " "); //�ش� idx + iĭ�� ������ �߰����ݴϴ�.
                continue;
            }
            s = s.Insert(idx[i], " "); //�ѹ��� ������ �ƴٸ� �� �ڸ��� ������ �߰����ݴϴ�.
        }

        string[] strs = s.Split(' '); //������ �������� �����ϴ�. (�׷��� �ܾ�� ������ �˴ϴ�)
        string returnStr = ""; //������ string ������ ������ְ�

        for (int i = 0; i < strs.Length; i++) //�ܾ�� ����ŭ �����ϴ�
        {
            returnStr += strs[i]; //������ �ܾ���� ��Ĩ�ϴ�
            if (i + 1 != strs.Length) returnStr += "_"; //�ݺ� �� �������� �ƴѶ�� _�� �߰����ݴϴ�.
        }


        return returnStr.ToUpper(); //���� �빮�ڷ� �ٲ㼭 return���ݴϴ�.
    }

    //���ϰ� ���� ���� �Լ����Դϴ�. �ڼ��� ������ ���� �ʵ��� �ϰڽ��ϴ�.
    public void SendData(string json)
    {
        webSocket.Send(json);
    }
    //���ϰ� ���� ���� �Լ����Դϴ�. �ڼ��� ������ ���� �ʵ��� �ϰڽ��ϴ�.
    public static void SendDataToSocket(string json)
    {
        Instance.SendData(json);
    }

    private void Update()
    {
        if (webSocket == null) return; //���� webSocket�� null�̶�� �� �ڵ尡 ��������ʰ� return ���ش�.

        if (packetList.Count > 0) //���� �������� �����Ͱ� �ͼ� packetList�� ���Դٸ�
        {
            IMsgHandler handler = null; //out���� �ޱ����� ����
            DataVO vo = packetList.Dequeue(); //�־�� ���� ���ϴ�.

            if (handlerDic.TryGetValue(vo.type, out handler)) //���� handlerDic�� vo.type�� �ش��ϴ� type�� �ڵ鷯�� �ִٸ� 
            {
                handler.HandleMsg(vo.payload); //�ش� handler�� �Լ��� �������ݴϴ�.
            }
            else
            {
                Debug.LogError($"�������� ���� �������� ��û {vo.type}");
                Debug.LogError(vo.payload);
            }
        }
    }

    public void ConnectSocket(string ip, int port)
    {
        //�ν��Ͻ� �ʱ�ȭ (ws://localhost:31012) <-�� url����
        webSocket = new WebSocket($"{url}:{port}");
        //���� �ʱ�ȭ �Ҷ� �����ص� url�� ������ �õ��Ѵ�.
        webSocket.Connect();

        //�������� �޽����� ������ ����Ǵ� event��.
        webSocket.OnMessage += (s, e) =>
        {
            DataVO vo = JsonUtility.FromJson<DataVO>(e.Data); //�� �����͸� DataVO�� �������� �ٲ۴�.
            packetList.Enqueue(vo); //packetList�� �־��ش�.
        };

    }
}