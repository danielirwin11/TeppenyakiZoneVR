/************************************************************************************

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// This transition will cause the screen to quickly fade to black, perform the repositioning, and then fade 
/// the view back to normal.
/// </summary>
public class TeleportTransitionFade : TeleportTransition
{
	/// <summary>
	/// How long the transition takes. Usually this is greater than Teleport Delay.
	/// </summary>
	[Tooltip("How long the transition takes. Usually this is greater than Teleport Delay.")]
	[Range(0.2f, 2.0f)]
	public float TransitionDuration = 0.5f;

	/// <summary>
	/// At what percentage of the elapsed transition time does the teleport occur?
	/// </summary>
	[Tooltip("How long should the screen stay black?")]
	[Range(0.0f, 1.0f)]
	public float StayFadedTime = 0.1f;

	/// <summary>
	/// When the teleport state is entered, start a coroutine that will handle the
	/// actual transition effect.
	/// </summary>
	protected override void LocomotionTeleportOnEnterStateTeleporting()
	{
		StartCoroutine(BlinkCoroutine());
	}

	/// <summary>
	/// This coroutine will fade out the view, perform the teleport, and then fade the view
	/// back in.
	/// </summary>
	/// <returns></returns>
	protected IEnumerator BlinkCoroutine()
	{
		LocomotionTeleport.IsTransitioning = true;
		OVRScreenFade.instance.fadeTime = TransitionDuration / 2f;
		OVRScreenFade.instance.FadeOut();
		yield return new WaitForSeconds(TransitionDuration);
		yield return new WaitForSeconds(StayFadedTime / 2f);
		LocomotionTeleport.DoTeleport();
		yield return new WaitForSeconds(StayFadedTime / 2f);
		OVRScreenFade.instance.FadeIn();
		LocomotionTeleport.IsTransitioning = false;
	}
}
