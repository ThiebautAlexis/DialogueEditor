using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class DialogAssetsManager
{
    private static string m_dialogAnswerHandlerName = "MultipleChoicesHandler";

    public static event Action<TextAsset> OnLineDescriptorLoaded;
    public static List<TextAsset> LineDescriptorsTextAsset;
    public static Dictionary<string, AudioClip> DialogLinesAudioClips = new Dictionary<string, AudioClip>();
    public static GameObject DialogAnswerHandler = null; 

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void LoadAssets()
    {
        Addressables.LoadAssetsAsync<TextAsset>("LineDescriptors", null).Completed += OnLineDescritorsLoaded;
        Addressables.LoadAssetsAsync<AudioClip>("AudioClips", null).Completed += OnAudioClipsLoaded;
        Addressables.LoadAssetAsync<GameObject>(m_dialogAnswerHandlerName).Completed += OnAnswerHandlerAssetLoaded;
    }

    /// <summary>
    /// Called when the AudioClips are loaded
    /// Add the loaded asset to <see cref="DialogLinesAudioClips"/> with its name as the key
    /// </summary>
    /// <param name="_loadedAssets"></param>
    private static void OnAudioClipsLoaded(AsyncOperationHandle<IList<AudioClip>> _loadedAssets)
    {

        if (_loadedAssets.Status == AsyncOperationStatus.Failed || _loadedAssets.Result == null || _loadedAssets.Result.Count == 0) return;
        for (int i = 0; i < _loadedAssets.Result.Count; i++)
        {
            DialogLinesAudioClips.Add(_loadedAssets.Result[i].name, _loadedAssets.Result[i]); 
        }
    }

    /// <summary>
    /// Called when the line descriptors are completly loaded 
    /// Call the event <see cref="OnLineDescriptorLoaded"/> after loading a line descriptor
    /// Add it to the list <see cref="LineDescriptorsTextAsset"/>
    /// </summary>
    /// <param name="_loadedAssets"></param>
    private static void OnLineDescritorsLoaded(AsyncOperationHandle<IList<TextAsset>> _loadedAssets)
    {
        LineDescriptorsTextAsset = new List<TextAsset>(); 
        for (int i = 0; i < _loadedAssets.Result.Count; i++)
        {
            LineDescriptorsTextAsset.Add(_loadedAssets.Result[i]); 
            OnLineDescriptorLoaded?.Invoke(_loadedAssets.Result[i]);
        }  
    }

    /// <summary>
    /// Called when the <paramref name="_answerHandler"/> is loaded with success. 
    /// Set the <see cref="m_dialogAnswerHandler"/> as the <paramref name="_answerHandler"/> Result
    /// </summary>
    /// <param name="_answerHandler">Loaded asset</param>
    private static void OnAnswerHandlerAssetLoaded(AsyncOperationHandle<GameObject> _answerHandler)
    {
        if (_answerHandler.Result == null)
        {
            Debug.LogError("IS NULL");
            return;
        }
        DialogAnswerHandler = _answerHandler.Result;
    }

}
