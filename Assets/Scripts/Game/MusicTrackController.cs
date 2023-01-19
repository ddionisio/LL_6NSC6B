using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrackController : M8.SingletonBehaviour<MusicTrackController> {
    [M8.MusicPlaylist]
    public string[] tracks;

    public bool isPlaying { get { return mRout != null; } }

    private Coroutine mRout;

    public void Play() {
        if(mRout != null)
            StopCoroutine(mRout);

        mRout = StartCoroutine(DoPlay());
    }

    public void Stop() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        M8.MusicPlaylist.instance.Stop(false);
    }

    IEnumerator DoPlay() {
        int curInd = 0;

        while(true) {
            var musicCtrl = M8.MusicPlaylist.instance;

            var curTrack = tracks[curInd];

            musicCtrl.Play(curTrack, false, false);

            while(musicCtrl.isPlaying)
                yield return null;

            curInd++;
            if(curInd == tracks.Length) {
                M8.ArrayUtil.Shuffle(tracks);
                curInd = 0;
            }

            yield return null;
        }
    }
}
