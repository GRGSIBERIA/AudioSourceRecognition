using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    AudioSource sound;

    /// <summary>
    /// �Đ��f�o�C�X�̃��X�g
    /// </summary>
    [SerializeField]
    List<string> devices;

    /// <summary>
    /// devices�̒��ŗ��p�������f�o�C�X��ID
    /// </summary>
    [SerializeField]
    int usingDeviceId = 0;

    /// <summary>
    /// �T���v�����O���g��
    /// </summary>
    [SerializeField]
    int samplingFrequency = 44100;

    /// <summary>
    /// �f�o�C�X��
    /// </summary>
    string deviceName;

    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<AudioSource>();

        devices = new List<string>(Microphone.devices);

        deviceName = devices[usingDeviceId];

        if (devices.Count > 0)
        {
            // �w�肵���T���v�����O���g����10�b�����o�b�t�@��݂���
            // ���[�v�^�����邱�ƂŃN���b�v�I�[�ɒB���Ă��N���b�v��[����^���𑱂���
            sound.clip = Microphone.Start(deviceName, true, 10, samplingFrequency);

            // �f�o�C�X���Đ������܂őҋ@
            if (Microphone.GetPosition(deviceName) <= 0) { }

            sound.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
