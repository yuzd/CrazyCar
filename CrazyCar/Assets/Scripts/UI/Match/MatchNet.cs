﻿using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Utils;
using QFramework;

public class MatchNet : MonoBehaviour, IController {
    private Coroutine matchNerCor;

    private void Start() {
        if (this.GetModel<IGameModel>().CurGameType == GameType.Match) {
            string ws = "ws" + this.GetSystem<INetworkSystem>().HttpBaseUrl.Substring(4) + 
                RequestUrl.matchWSUrl +
                this.GetModel<IUserModel>().Uid.Value + "," +
                this.GetModel<IMatchModel>().SelectInfo.Value.cid;

            if (this.GetSystem<INetworkSystem>().NetType == NetType.WebSocket) {
                this.GetSystem<INetworkSystem>().Connect(ws);
            } else if (this.GetSystem<INetworkSystem>().NetType == NetType.KCP) {
                this.GetSystem<INetworkSystem>().Connect(Util.GetServerHost(this.GetSystem<INetworkSystem>().ServerType));
            }

            //等待都进入后 再发送创建人物消息
            Util.DelayExecuteWithSecond(3, () => { this.SendCommand<PostCreatePlayerMsgCommand>(); });
            Util.DelayExecuteWithSecond(4.5f, () => { matchNerCor = CoroutineController.manager.StartCoroutine(SendMsg()); }); 
        }

        this.RegisterEvent<ExitGameSceneEvent>(OnExitGameScene).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnExitGameScene(ExitGameSceneEvent e)
    {
        if (matchNerCor != null)
        {
            CoroutineController.manager.StopCoroutine(matchNerCor);
        }
    }

    private IEnumerator SendMsg() {
        while (true) {
            this.SendCommand<PostPlayerStateMsgCommand>();
            yield return new WaitForSeconds(this.GetModel<IGameModel>().SendMsgOffTime.Value);
        }
    }

    private void OnDestroy() {
        this.GetSystem<INetworkSystem>().CloseConnect();
    }

    public IArchitecture GetArchitecture() {
        return CrazyCar.Interface;
    }
}
