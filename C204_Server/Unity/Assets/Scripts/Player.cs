using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isRemote; //���� �����ϴ� ĳ�����Ͻ� false 
    public int socketId; //�� ĳ������ socketId;

    private float speed = 5f;

    private float lerpSpeed = 4f;

    private float h;
    private float v;

    private Vector3 dir;

    private Vector2 targetPos; //���� �����ϴ� ĳ���Ͱ� �ƴҰ�� �������� ���� position ������ �־��ش�

    private WaitForSeconds ws = new WaitForSeconds(1 / 5); //200ms�ֱ�� ������ ������

    private void Start()
    {

    }

    private void Update()
    {
        if (isRemote) //���� �����ϴ� ĳ���Ͱ� �ƴ϶�� �� ĳ������ position�� �ε巴�� �̵������ش�.
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
        }
        else
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) //wasd, ����Ű�� ������ ��
            {
                //���� �޾ƿͼ�
                h = Input.GetAxisRaw("Horizontal");
                v = Input.GetAxisRaw("Vertical");

                //���� �̵��ؾ��ϴ��� ���⸸ �޴´�.
                dir = new Vector3(h, v, 0).normalized;
                //�� �������� �̵����ش�.
                Move(dir);
            }
        }
    }

    public void InitPlayer(TransformVO vo, bool isRemote) //�÷��̾� ������ ����� �Ұ͵�
    {
        this.socketId = vo.socketId;
        this.isRemote = isRemote;

        transform.position = vo.position;

        if (!isRemote)
        {
            StartCoroutine(SendData()); //���� ���� �����ϴ� ĳ���Ͷ�� ������ �����͸� �����ش�.
        }
    }

    IEnumerator SendData()
    {
        while (true)
        {
            TransformVO vo = new TransformVO();  //���� position�� ���� ������ �ִ´�.
            vo.socketId = socketId;
            vo.position = transform.position;

            DataVO dataVO = new DataVO("TRANSFORM", JsonUtility.ToJson(vo));  //TRANSFORM�̶�� Ÿ��, ���� ���� vo�� jsonȭ �Ͽ� dataVO�� ������ش�.

            SocketClient.SendDataToSocket(JsonUtility.ToJson(dataVO)); //�װ� �ѹ� �� jsonȭ ���� ������ ������.

            yield return null;
        }
    }

    public void Move(Vector3 dir)
    {
        if (isRemote) return; //������ playerinput, playermove, ���� ��ũ��Ʈ ������ ������ input�� move��ũ��Ʈ enable�� ���ų� Ű�� ����� �����ϴ�.
        transform.Translate(dir * speed * Time.deltaTime); //�� �������� �̵������ش�.
    }

    public void SetTransform(Vector2 pos) //���� �����ϴ� ĳ���Ͱ� �ƴҰ�� �������� ���� position�� target�� ����������Ѵ�.
    {
        if (isRemote)
        {
            targetPos = pos;
        }
    }
}
