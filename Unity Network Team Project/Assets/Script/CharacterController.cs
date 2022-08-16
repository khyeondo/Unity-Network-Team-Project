using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class CharacterController : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform cameraBasisPos;
    public float maxSpeed = 5f;
    public float diveSpeed = 20f;

    private BoxCollider hitBox;
    public bool isOnGround = true;
    public bool isChargingAttack = false;    // 차지 공격용
    private ParticleController particleController;

    private float damaged = 2f;
    private float damagedLimit = 4f;
    private Rigidbody rigidbody;
    private Animator animator;
    private Transform meshTransform;
    private Vector3 diveDir;

    private float timePressed_AttackR, pressTime = 0.0f;
    public float countToCharge = 1.0f;
    private bool isKeyPressed = false;

    private PhotonView pv;
    public int actorNumber = -1;

    #region 네트워크 동기화를 위한 멤버변수
    Vector3 curPos;
    Quaternion curRotation;
    #endregion

    #region UI
    private GameObject gameMgr;
    private GameObject scrollView;
    public GameObject characterImagePrefab;
    private GameObject characterImage;
    private Slider slider;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        meshTransform = GetComponentInChildren<Transform>();
        rigidbody = GetComponent<Rigidbody>();
        hitBox = GetComponent<BoxCollider>();
        particleController = GetComponent<ParticleController>();
        pv = GetComponent<PhotonView>();
        hitBox.enabled = false;

        if (pv.IsMine == false)
        {
            animator.applyRootMotion = false;
        }
        else
        {
            actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            pv.RPC("SetActorNumber", RpcTarget.Others, actorNumber);
        }

        //UI
        scrollView = GameObject.FindGameObjectWithTag("ScrollView");
        characterImage = Instantiate(characterImagePrefab, scrollView.transform);
        slider = characterImage.transform.GetChild(1).GetComponent<Slider>();
        slider.value = 0f;

        characterImage.transform.GetChild(0).GetComponent<Text>().text = pv.Owner.NickName;
        transform.GetChild(3).GetComponent<TextMesh>().text = pv.Owner.NickName;


        PhotonManagerInGame.inGameManager.AddPlayer(gameObject);
    }

    [PunRPC]
    void SetActorNumber(int actorNumber)
    {
        this.actorNumber = actorNumber;
    }


    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine == true)
        {
            Move();
            Jump();
            Attack();
            Dive();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, curPos, 20f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, curRotation, 20f * Time.deltaTime);
        }

        LimitDamaged();
        float percent = (damaged - 2) / (damagedLimit - 2f);
        slider.value = percent;
    }

    private void OnDestroy()
    {
        Destroy(characterImage);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.gameObject != gameObject)
        {
            //OnKnockBack(other);
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("standard attack") ||
                animator.GetCurrentAnimatorStateInfo(0).IsName("combo attack 1"))
            {
                other.GetComponent<CharacterController>().QuickAttacked(gameObject);
                particleController.HitEffect(other);
            }
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("combo attack 2"))
            {
                other.GetComponent<CharacterController>().HardAttacked(gameObject);
                particleController.HitEffect(other);
            }
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("charge attack"))
            {
                other.GetComponent<CharacterController>().HardAttacked(gameObject);
                particleController.ChargeHitEffect(other);
            }
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("power attack"))
            {
                other.GetComponent<CharacterController>().UpperAttacked(gameObject);
                particleController.UpperHitEffect(other);
            }

        }

        if (other.tag == "Border")
        {
            OnDeath();
        }

        if (other.gameObject.tag == "Ground")
        {
            isOnGround = true;
        }

        if(other.gameObject.tag == "item")
        {
            if(pv.IsMine)
            {
                PhotonManagerInGame.inGameManager.RequestGetItem();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isOnGround = false;
        }
    }

    IEnumerator OnRootMotion()
    {
        yield return new WaitForSeconds(2f * damaged);
        animator.applyRootMotion = true;
    }

    public void OnKnockBack(Collider other)
    {

    }

    void OnDeath()
    {
        PhotonManagerInGame.inGameManager.OnDeathPlayer(gameObject);
        particleController.DeathEffect();
        Destroy(gameObject);
    }

    #region 공격 판정
    //스턴
    void QuickAttacked(GameObject attacker)
    {
        transform.forward = attacker.transform.forward * -1f;
        SyncAnimTrigger("quickAttacked");
        damaged += 0.1f;
    }

    //넉백
    void HardAttacked(GameObject attacker)
    {
        rigidbody.velocity = Vector3.zero;
        transform.forward = attacker.transform.forward * -1f;
        transform.eulerAngles += new Vector3(0f, -30f, 0f);

        Vector3 dir = attacker.transform.forward;
        dir.y = 0f;
        IsMineAddForce((dir.normalized * 4f + Vector3.up * 2f) * damaged, ForceMode.Impulse);

        
        damaged += 0.2f;
        SyncAnimTrigger("hardAttacked");
    }

    //위로 띄우기
    void UpperAttacked(GameObject attacker)
    {
        transform.forward = attacker.transform.forward * -1f;
        transform.eulerAngles += new Vector3(0f, -30f, 0f);

        Vector3 dir = attacker.transform.forward;
        dir.y = 0f;
        IsMineAddForce((dir.normalized * 1f + Vector3.up * 5f) * damaged, ForceMode.Impulse);

        //SyncedAddForce((dir.normalized * 1f + Vector3.up * 5f) * damaged, ForceMode.Impulse);        

        damaged += 0.2f;
        SyncAnimTrigger("upperAttacked");

    }

    public void IsMineAddForce(Vector3 dir, ForceMode mode)
    {
        if (pv.IsMine)
            rigidbody.AddForce(dir, mode);
    }
    #endregion


    #region 공격
    void Attack()
    {
        //마우스 왼쪽 = 약공격
        if (Input.GetAxis("AttackL") == 1)
        {
            SyncAnimTrigger("attack");
            ComboAttack();
        }

        //마우스 오른쪽 = 강공격 or 차지공격
        if (Input.GetKeyDown(KeyCode.Mouse1) && !isKeyPressed)
        {
            timePressed_AttackR = Time.time;
            pressTime = timePressed_AttackR + countToCharge;
            isKeyPressed = true;
        }
        PowerAttack();
        ChargeAttack();
    }

    void PowerAttack()
    {
        //마우스 오른쪽 버튼을 한번만 누르면
        //강공격 활성화
        //강공격은 띄우기 판정
        if (Input.GetKeyUp(KeyCode.Mouse1) && Time.time < pressTime)
        {
            isKeyPressed = false;
            SyncAnimTrigger("attack_power");
        }
    }

    void ChargeAttack()
    {
        //마우스 오른쪽 버튼을 countDown만큼의 시간 동안 누르고 있으면
        //차지공격 활성화
        if (Input.GetKey(KeyCode.Mouse1) && Time.time >= pressTime)
        {
            isKeyPressed = false;
            SyncAnimTrigger("attack_charge");
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("charge attack") &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
            {
                SyncAnimFloat("charge_attack_anim_speed", 0);
                isChargingAttack = true;
            }
        }
        if (!Input.GetKey(KeyCode.Mouse1) && isChargingAttack)
        {
            SyncAnimFloat("charge_attack_anim_speed", 1);
            isChargingAttack = false;
        }
    }

    void ComboAttack()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("standard attack") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
        {
            SyncAnimTrigger("combo_attack1");
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("combo attack 1") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
        {
            SyncAnimTrigger("combo_attack2");
        }
    }

    #endregion

    #region 이동
    void Move()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("run"))
        {

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 moveDir = (cameraBasisPos.forward * v) + (cameraBasisPos.right * h);
            //transform.Translate(moveDir.normalized * Time.deltaTime * maxSpeed);
            //
            if (h != 0f || v != 0f)
            {
                transform.forward = moveDir;
                SyncAnimBool("isRun", true);
                //animator.SetFloat("right", h);
                //animator.SetFloat("forward", v);
            }
            else
            {
                SyncAnimBool("isRun", false);
            }
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("run") || animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                SyncAnimTrigger("jump");
                rigidbody.velocity = Vector3.zero;
                rigidbody.AddForce(Vector3.up * 17f, ForceMode.Impulse);
            }
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("jump"))
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            transform.forward = cameraBasisPos.forward;
            Vector3 moveDir = (transform.forward * v) + (transform.right * h);
            transform.position += moveDir * Time.deltaTime * 10f;
        }
    }

    void Dive()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("run") || animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                SyncAnimTrigger("dive");
                diveDir = (Vector3.forward * Input.GetAxis("Vertical")) + (Vector3.right * Input.GetAxis("Horizontal"));
            }
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("dive"))
        {
            animator.applyRootMotion = true;
            //transform.forward = diveDir;
            //transform.Translate(diveDir.normalized * Time.deltaTime * diveSpeed);
        }
    }
    #endregion

    #region 콜리전 활성화/비활성화
    void OnHitBox()
    {
        hitBox.enabled = true;


    }
    void OffHitBox()
    {
        hitBox.enabled = false;
    }
    #endregion

    #region 동기화를 위한 함수
    [PunRPC] void RPCAnimTrigger(string str) => animator.SetTrigger(str);
    [PunRPC] void RPCAnimBool(string str, bool b) => animator.SetBool(str, b);
    [PunRPC] void RPCAnimFloat(string str, float f) => animator.SetFloat(str, f);

    public void SyncAnimTrigger(string str)
    {
        animator.SetTrigger(str);
        pv.RPC("RPCAnimTrigger", RpcTarget.Others, str);
    }
    void SyncAnimBool(string str, bool b)
    {
        if(pv != null)
        {
            animator.SetBool(str, b);
            pv.RPC("RPCAnimBool", RpcTarget.Others, str, b);
        }
    }
    void SyncAnimFloat(string str, float f)
    {
        animator.SetFloat(str, f);
        pv.RPC("RPCAnimFloat", RpcTarget.Others, str, f);        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            curRotation = (Quaternion)stream.ReceiveNext();
        }
    }
    #endregion

    //누적데미지 getter
    public float AccumulateDamage
    {
        get { return damaged; }
    }

    public void LimitDamaged()
    {
        damaged = Mathf.Min(damaged, damagedLimit);
    }
}