using System.Collections;
using UnityEngine;

public class ClearSweet : MonoBehaviour {

    public AnimationClip animClip;
    // 正在删除的状态
    private bool isClearing = false;

    public AudioClip clearClip;

    public bool IsClearing
    {
        get
        {
            return isClearing;
        }
    }
    private GameSweet sweet;
    public GameSweet Sweet
    {
        get
        {
            return sweet;
        }

        set
        {
            sweet = value;
        }
    }

    public virtual void Clear()
    {
        isClearing = true;
        StartCoroutine(PlayClearAni());
    }
    // 播放消除动画
    private IEnumerator PlayClearAni()
    {
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            GameManager.Instance.Score++;
            animator.Play(animClip.name);
            // 播放音效
            AudioSource.PlayClipAtPoint(clearClip, transform.position);
            // 获取动画时长
            yield return new WaitForSeconds(animClip.length);
            Destroy(gameObject);
        }
    }
}
