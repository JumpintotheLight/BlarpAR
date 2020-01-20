using UnityEngine;
using System.Collections;
//
using Mirror;

public interface IUsable
{
    //void StartUsing(uint handId);
    //void StopUsing(uint handId);
    void StartUsing(uint handId);
	void StopUsing(uint handId);
}
