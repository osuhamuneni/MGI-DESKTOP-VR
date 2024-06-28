using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GalaxyDataUI : MonoBehaviour
{
    public TextMeshProUGUI header;
    public TextMeshProUGUI data;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetData(string header, string data)
    {
        this.header.text = header;
        this.data.text = data;
    }
}
