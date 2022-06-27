using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

public class Atari : MonoBehaviour
{
    private AudioSource sound01;
    private Thread thread;
    private bool isRunning = false;
    private string message;
    private bool isNewMessageReceived = false;
    public GameObject[] tap;

    void Start()
    {
        sound01 = GetComponent<AudioSource>();
        Open();
    }

    //void OnDestroy()
    //{
    //    Close();
    //}

    private void OnApplicationQuit()
    {
        Close();
    }


    private void Close()
    {
        isNewMessageReceived = false;
        isRunning = false;

        //if (thread != null && thread.IsAlive)
        //{
        //    thread.Join();
        //}

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            serialPort.Dispose();
        }
    }

    private string gk(string c){
        return Convert.ToString(Convert.ToInt32(Input.GetKey(c)));
    }
    private string gc(UnityEngine.KeyCode c){
        return Convert.ToString(Convert.ToInt32(Input.GetKey(c)));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            sound01.PlayOneShot(sound01.clip);
        }
        if(isRunning && (serialPort == null || !serialPort.IsOpen)){
            message = gc(KeyCode.Tab) + gc(KeyCode.Tab) + gk("q") + gk("w") + gk("e") + gk("r") + gk("t") + gk("y") + gk("u") + gk("i") + gk("o") + gk("p") + gk("@") + gk("[") + gc(KeyCode.Return) + gc(KeyCode.Return);
            isNewMessageReceived = true;
        }
        if (isNewMessageReceived)
        {
            char[] cs;
            if(message is String && message.ToCharArray().Length == 16){
                cs = message.ToCharArray();
            }else {
                cs = Convert.ToString(Convert.ToInt32(message, 16), 2).ToCharArray();
                Array.Reverse(cs);
                cs = new String(cs).PadRight(16, '0').ToCharArray();
                Array.Reverse(cs);
            }


            for (int i = 0; i < cs.Length; i++)
                if (cs[i] == '0'){
                    try{
                        tap[i].SetActive(false);
                    }catch(Exception e){
                        Debug.LogWarning(Convert.ToString(i) + " " + cs.Length + " " + new String(cs));
                    }
                }
                else if (cs[i] == '1'){
                    tap[i].SetActive(true);
                }

            String color = "";
            for (int i = 0; i < cs.Length / 4; i++)
                if (cs[i * 4] == '1' || cs[i * 4 + 1] == '1' || cs[i * 4 + 2] == '1' || cs[i * 4 + 3] == '1')
                    color = "1111" + color;
                else
                    color = "0000" + color;

            Write(color+";");
            Debug.Log(color);
            //char[] cs = message.ToCharArray();

                //for (int i = 0; i < cs.Length; i++)
                //    if (cs[i] == '0')
                //        tap[i].SetActive(false);
                //    else if (cs[i] == '1')
                //        tap[i].SetActive(true);
        }
        isNewMessageReceived = false;
    }

    int[] led = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogWarning("OnCollisionEnter");
        if (collision.gameObject.tag == "Notes")
        {
            sound01.PlayOneShot(sound01.clip);
            for (int i = collision.gameObject.GetComponent<Notes>().nm.start; i < collision.gameObject.GetComponent<Notes>().nm.width + 1; i++)
            {
                led[i] = 1;
            }
            string str = "";
            foreach (int i in led) str += i;
            Write(str + ";");
        }
        else
        {
            Debug.Log(collision.contacts[0].point.x);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Notes")
        {

            for (int i = collision.gameObject.GetComponent<Notes>().nm.start; i < collision.gameObject.GetComponent<Notes>().nm.width + 1; i++)
            {
                led[i] = 0;
            }
            string str = "";
            foreach (int i in led) str += i;
            Write(str + ";");
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        //Debug.LogWarning("OnCollisionStay");
       foreach(ContactPoint cp in collision.contacts)
       {
           //Debug.LogWarning(cp.point.x);
       }
    }

    private SerialPort serialPort = null;
    private void Open()
    {
        try{
        serialPort = new SerialPort(@"COM3", 2000000); // hardcoding may not be good
        serialPort.Open();                             // gives error if COM3 not found
        Debug.Log("OPEN PORT");                        // must try/catch and use keyboard instead
        
        isRunning = true;
        thread = new Thread(Read);
        thread.Start();
        }
        catch(System.IO.IOException e)
        {
            Debug.LogWarning("Probably no COM3; falling back to keyboard" + e.Message);
            isRunning = true;

        }
        finally {
        }

    }

    private void Read()
    {
        while (isRunning&& serialPort != null && serialPort.IsOpen)
        {
            try
            {
                message = serialPort.ReadLine();
                isNewMessageReceived = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        // while(isRunning && (serialPort == null || !serialPort.IsOpen)) {
        //     Debug.LogWarning("serialPort is null");
        //     if(Input.GetKey(KeyCode.A)){
        //         Debug.LogWarning("KeyA");
        //         message = "11111111";
        //         isNewMessageReceived = true;
        //     }
        // }
    }

    public void Write(string message)
    {
        if(serialPort != null && serialPort.IsOpen){
        try
        {
            serialPort.Write(message);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        }
    }
}