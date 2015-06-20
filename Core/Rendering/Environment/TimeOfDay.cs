using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class TimeOfDay : MonoBehaviour {

    public float m_Time = 9.0f;
    public float m_SunriseTime = 7.50f;
    public float m_SunsetTime = 20.50f;
    protected Vector3 m_LightDirection = new Vector3(0, 1, 0);
    public Vector3 LightDirection { get { return m_LightDirection; } }

    public void CalculationTime()
    {
        if (m_Time > 24.0f)
            m_Time -= 24.0f;
        else if (m_Time < 0.0f)
            m_Time += 24.0f;

        if (m_SunriseTime > m_SunsetTime)
            m_SunriseTime = m_SunsetTime;

        float y,
            X = m_Time,
            A = m_SunriseTime,
            B = m_SunsetTime,
            AB = A + 24 - B,
            AB_ = B - A,
            XB = X + 24 - B;

        if (X < A || X > B)
        {
            if (X < A)
            {
                y = -XB / AB;
            }
            else
            {
                y = -(X - B) / AB;
            }

            if (y > -0.5f)
            {
                y *= 2;
            }
            else
            {
                y = -(1 + y) * 2;
            }
        }
        else
        {
            y = (X - A) / (B - A);

            if (y < 0.5f)
            {
                y *= 2;
            }
            else
            {
                y = (1 - y) * 2;
            }
        }

        Vector2 East = new Vector2(0, 1);

        if (X > A && X < B)
        {
            if (X > (A + AB_ / 2))
            {
                East = -East;
            }
        }
        else
        {
            if (X <= A)
            {
                if (XB < (24 - AB_) / 2)
                {
                    East = -East;
                }
            }
            else
            {
                if ((X - B) < (24 - AB_) / 2)
                {
                    East = -East;
                }
            }
        }


        float ydeg = (Mathf.PI / 2) * y,
              sn = Mathf.Sin(ydeg),
              cs = Mathf.Cos(ydeg);

        m_LightDirection.x = East.x * cs;
        m_LightDirection.y = sn;
        m_LightDirection.z = East.y * cs;

        gameObject.transform.position = m_LightDirection * 100000.0f;
        gameObject.transform.rotation = Quaternion.LookRotation(-m_LightDirection);
    }

    void Awake()
    {
        CalculationTime();
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
