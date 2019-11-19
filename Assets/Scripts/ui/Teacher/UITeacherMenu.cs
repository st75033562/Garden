using UnityEngine;
using System.Collections;

public class UITeacherMenu : MonoBehaviour {
    [SerializeField]
    private GameObject taskPool;
    [SerializeField]
    private GameObject systemTemplate;

	// Use this for initialization
	void Start () {
        if (UserManager.Instance.IsTeacher)
        {
            taskPool.SetActive(true);
            systemTemplate.SetActive(true);
        }
    }
}
