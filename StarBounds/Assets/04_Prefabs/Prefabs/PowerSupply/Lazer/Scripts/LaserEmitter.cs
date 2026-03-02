using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    [Header("레이저 설정")]
    public bool isFiring = false;
    public float maxDistance = 50f;
    public int maxBounces = 10;
    public LayerMask collisionLayer;

    [Header("비주얼 컴포넌트")]
    public LineRenderer lineRenderer;
    public Transform startSpriteObj;
    public Transform endSpriteObj;
	void Update()
	{
		if (isFiring)
		{
			DrawLaser();
		}
	}
    public void SetLaserState(bool state)
    {
        isFiring = state;
        if (!isFiring)
        {
            lineRenderer.positionCount = 0;
            startSpriteObj.gameObject.SetActive(false);
            endSpriteObj.gameObject.SetActive(false);
        }
    }
	private void DrawLaser()
	{
		List<Vector2> laserPoints = new List<Vector2>();
		Vector2 currentPosition = transform.position;
		Vector2 currentDirection = transform.right; // 총구가 바라보는 방향

		laserPoints.Add(currentPosition);

		for (int i = 0; i < maxBounces; i++)
		{
			// 현재 위치에서 광선 발사
			RaycastHit2D hit = Physics2D.Raycast(currentPosition, currentDirection, maxDistance, collisionLayer);

			if (hit.collider != null)
			{
				laserPoints.Add(hit.point);

				// 부딪힌 오브젝트가 거울(Mirror) 태그를 가지고 있다면 반사!
				if (hit.collider.CompareTag("Mirror"))
				{
					// 입사각과 거울의 법선(Normal)을 계산해 반사각(Reflect)을 구함
					currentDirection = Vector2.Reflect(currentDirection, hit.normal);

					// 미세하게 앞으로 전진시켜 자가 충돌 방지
					currentPosition = hit.point + currentDirection * 0.05f;
				}
				else
				{
					// 거울이 아닌 벽, 바닥, 플레이어에 닿으면 여기서 정지
					break;
				}
			}
			else
			{
				// 허공이면 끝까지 쏨
				laserPoints.Add(currentPosition + currentDirection * maxDistance);
				break;
			}
		}

		// --- 시각적 업데이트 (비주얼) ---

		// 1. 중간 선 (Line Renderer) 업데이트
		lineRenderer.positionCount = laserPoints.Count;
		for (int i = 0; i < laserPoints.Count; i++)
		{
			lineRenderer.SetPosition(i, laserPoints[i]);
		}

		// 2. 시작 스프라이트 위치/활성화
		startSpriteObj.gameObject.SetActive(true);
		startSpriteObj.position = laserPoints[0];

		// 3. 끝점 스프라이트 위치/회전/활성화 (어딘가에 부딪혔을 때만 끝점 표시)
		if (laserPoints.Count > 1)
		{
			endSpriteObj.gameObject.SetActive(true);
			endSpriteObj.position = laserPoints[laserPoints.Count - 1];

			// 끝점 스프라이트가 레이저가 날아온 방향을 바라보게 회전
			Vector2 finalDir = laserPoints[laserPoints.Count - 1] - laserPoints[laserPoints.Count - 2];
			float angle = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
			endSpriteObj.rotation = Quaternion.Euler(0, 0, angle);
		}
	}
}
