using UnityEngine;
using System.Collections;
//
using Mirror;

public interface ITouchable {
	void Touch (uint handId);
	void Untouch (uint handId);
}
