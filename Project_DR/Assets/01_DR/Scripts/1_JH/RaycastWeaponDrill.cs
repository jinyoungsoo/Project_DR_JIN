﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BNG
{


    public class RaycastWeaponDrill : GrabbableEvents
    {

        [Header("General")]

        public bool isDrill;

        // 최대 사거리
        public float MaxRange = 25f;

        // 데미지 : "Damageable" 함수와 충돌했을 경우 입힐 데미지
        public float Damage = 25f;

        // Semi : 단발 ,  Automatic : 자동 연사 설정
        public FiringType FiringMethod = FiringType.Semi;


        // 탄창 설정
        // InfiniteAmmo : 무한, Manual Clip : 탄창 활용 장전
        public ReloadType ReloadMethod = ReloadType.InfiniteAmmo;

        /// 연사 설정 : 0.2 = 초당 5발 
        public float FiringRate = 0.2f;
        float lastShotTime;

        // 데미지를 입힐 때 리지드바디에 넘겨줄 총알의 힘
        // 총알에 맞은 "Damageable" 물체는 아래 힘만큼 밀린다.
        public float BulletImpactForce = 1000f;

        // FiringType InternalAmmo일 경우, 이 무기의 InternalAmmo
        // ex. 1발씩 발사하는 샷건 같은 종류
        public float InternalAmmo = 0;

        // 이 무기가 수용할 수 있는 최대 InternalAmmo의 양
        public float MaxInternalAmmo = 10;

        // 탄을 발사했을 때 자동으로 장전될 경우 체크. 볼트액션, 펌프 샷건은 체크 해제
        public bool AutoChamberRounds = true;

        // 탄약을 장착하고 장전을 해야하는 경우 체크
        // 바로 발사하려면 false
        public bool MustChamberRounds = false;

        [Header("Projectile Settings")]

        // True인 경우, 레이캐스트 대신 발사체가 발사
        public bool AlwaysFireProjectile = false;

        // True인 경우 레이캐스트를 사용하는 대신 슬로우 모션 중에 ProjectilePrefab이 인스턴스화
        public bool FireProjectileInSlowMo = true;

        // 슬로 모션 중에 무기를 발사하는 속도. Time.timeScale의 영향을 받음
        public float SlowMoRateOfFire = 0.3f;

        // 발사체에 적용할 힘의 양
        public float ShotForce = 10f;

        // 탄피 프리팹에 적용할 힘의 양
        public float BulletCasingForce = 3f;

        [Header("Recoil")]
        
        // 총기 발사 시 반동 힘
        public Vector3 RecoilForce = Vector3.zero;

        // 반동 지속 시간
        public float RecoilDuration = 0.3f;

        Rigidbody weaponRigid;

        [Header("Raycast Options")]
        public LayerMask ValidLayers;

        [Header("Weapon Setup")]
        // 트리거 위치
        public Transform TriggerTransform;


        // 발사 시 움직일 총 슬라이드
        public Transform SlideTransform;

        // 발사체 또는 Ray가 발사될 위치
        public Transform MuzzlePointTransform;

        // 탄피가 날아갈 위치
        public Transform EjectPointTransform;

        // 약실에 있을 발사체, 없다면 제거
        public Transform ChamberedBullet;

        // 발사 시 반짝이는 이펙트
        public GameObject MuzzleFlashObject;

        // 날아갈 탄피 prefab
        public GameObject BulletCasingPrefab;

        // 발사체 prefab
        public GameObject ProjectilePrefab;

        // 발사체가 충돌한 지점에 생길 충돌 효과 Prefab
        public GameObject HitFXPrefab;

        // 발사 효과음
        public AudioClip GunShotSound;

        // 발사 효과음 볼륨
        [Range(0.0f, 1f)]
        public float GunShotVolume = 0.75f;

        // 빈 탄창 효과음
        public AudioClip EmptySound;

        // 빈 탄창 효과음 볼륨
        [Range(0.0f, 1f)]
        public float EmptySoundVolume = 1f;

        [Header("Slide Configuration : ")]
        // 발사 시 슬라이드 위치가 얼마나 뒤로갈지 설정
        public float SlideDistance = -0.028f;

        // 마지막 총알을 발사했을 경우, 슬라이드가 뒤에있어야 하는지
        public bool ForceSlideBackOnLastShot = true;

        // 발사 시 슬라이드의 속도
        public float slideSpeed = 1;

        // 슬라이드의 최소 거리
        float minSlideDistance = 0.001f;

        [Header("Inputs : ")]
        [Tooltip("Controller Input used to eject clip")]
        public List<GrabbedControllerBinding> EjectInput = new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };

        [Tooltip("Controller Input used to release the charging mechanism.")]
        public List<GrabbedControllerBinding> ReleaseSlideInput = new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button1Down };

        [Tooltip("Controller Input used to release reload the weapon if ReloadMethod = InternalAmmo.")]
        public List<GrabbedControllerBinding> ReloadInput = new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };

        [Header("Shown for Debug : ")]
        // 탄이 약실에 들어있는지
        public bool BulletInChamber = false;

        /// <summary>
        /// Is there currently a bullet chambered and that must be ejected
        /// </summary>
        [Tooltip("Is there currently a bullet chambered and that must be ejected")]
        public bool EmptyBulletInChamber = false;

        [Header("Events")]

        [Tooltip("Unity Event called when Shoot() method is successfully called")]
        public UnityEvent onShootEvent;

        [Tooltip("Unity Event called when something attaches ammo to the weapon")]
        public UnityEvent onAttachedAmmoEvent;

        [Tooltip("Unity Event called when something detaches ammo from the weapon")]
        public UnityEvent onDetachedAmmoEvent;

        [Tooltip("Unity Event called when the charging handle is successfully pulled back on the weapon")]
        public UnityEvent onWeaponChargedEvent;

        [Tooltip("Unity Event called when weapon damaged something")]
        public FloatEvent onDealtDamageEvent;

        [Tooltip("Passes along Raycast Hit info whenever a Raycast hit is successfully detected. Use this to display fx, add force, etc.")]
        public RaycastHitEvent onRaycastHitEvent;

        /// <summary>
        /// Is the slide / receiver forced back due to last shot
        /// </summary>
        protected bool slideForcedBack = false;

        protected WeaponSlide ws;

        protected bool readyToShoot = true;

        // 드릴 작동을 위한 클래스
        protected WeaponDrill drill;

        void Start()
        {
            weaponRigid = GetComponent<Rigidbody>();

            if (MuzzleFlashObject)
            {
                MuzzleFlashObject.SetActive(false);
            }
            
            ws = GetComponentInChildren<WeaponSlide>();
            drill = GetComponentInChildren<WeaponDrill>();

            updateChamberedBullet();
        }

        public override void OnTrigger(float triggerValue)
        {


            // Sanitize for angles 
            triggerValue = Mathf.Clamp01(triggerValue);

            // Update trigger graphics
            if (TriggerTransform)
            {
                TriggerTransform.localEulerAngles = new Vector3(triggerValue * 15, 0, 0);
            }

            // Trigger up, reset values
            if (triggerValue <= 0.5)
            {
                readyToShoot = true;
                playedEmptySound = false;
            }

            // Fire gun if possible
            if (readyToShoot && triggerValue >= 0.75f)
            {
                Shoot();

                // Immediately ready to keep firing if 
                readyToShoot = FiringMethod == FiringType.Automatic;
            }

            // These are here for convenience. Could be called through GrabbableUnityEvents instead
            checkSlideInput();
            checkEjectInput();
            CheckReloadInput();

            updateChamberedBullet();

            base.OnTrigger(triggerValue);
        }

        void checkSlideInput()
        {
            // Check for bound controller button to release the charging mechanism
            for (int x = 0; x < ReleaseSlideInput.Count; x++)
            {
                if (InputBridge.Instance.GetGrabbedControllerBinding(ReleaseSlideInput[x], thisGrabber.HandSide))
                {
                    UnlockSlide();
                    break;
                }
            }
        }

        void checkEjectInput()
        {
            // Check for bound controller button to eject magazine
            for (int x = 0; x < EjectInput.Count; x++)
            {
                if (InputBridge.Instance.GetGrabbedControllerBinding(EjectInput[x], thisGrabber.HandSide))
                {
                    EjectMagazine();
                    break;
                }
            }
        }

        public virtual void CheckReloadInput()
        {
            if (ReloadMethod == ReloadType.InternalAmmo)
            {
                // Check for Reload input(s)
                for (int x = 0; x < ReloadInput.Count; x++)
                {
                    if (InputBridge.Instance.GetGrabbedControllerBinding(EjectInput[x], thisGrabber.HandSide))
                    {
                        Reload();
                        break;
                    }
                }
            }
        }

        public virtual void UnlockSlide()
        {
            if (ws != null)
            {
                ws.UnlockBack();
            }
        }

        public virtual void EjectMagazine()
        {
            MagazineSlide ms = GetComponentInChildren<MagazineSlide>();
            if (ms != null)
            {
                ms.EjectMagazine();
            }
        }

        protected bool playedEmptySound = false;


        // 발사 부분
        public virtual void Shoot()
        {


            // 사격 가능한지 확인
            float shotInterval = Time.timeScale < 1 ? SlowMoRateOfFire : FiringRate;
            // 마지막 사격 시간을 체크해 shotInterval보다 낮으면 리턴
            if (Time.time - lastShotTime < shotInterval)
            {
                return;
            }

            // 탄약을 장전하고 쏴야하는 총일 경우, 총알이 비었을 때
            if (!BulletInChamber && MustChamberRounds)
            {
                // 사운드가 안 비어있을 경우
                if (!playedEmptySound)
                {
                    // 빈 탄창 사운드 재생
                    VRUtils.Instance.PlaySpatialClipAt(EmptySound, transform.position, EmptySoundVolume, 0.5f);
                    playedEmptySound = true;
                }

                return;
            }

            // 슬라이드가 있고, 슬라이드가 뒤에 멈춰있는 경우(마지막 총을 쐈을 때)
            if (ws != null && ws.LockedBack)
            {
                VRUtils.Instance.PlaySpatialClipAt(EmptySound, transform.position, EmptySoundVolume, 0.5f);
                return;
            }

            // 사격 사운드 재생
            VRUtils.Instance.PlaySpatialClipAt(GunShotSound, transform.position, GunShotVolume);

            // 진동 (뭔가 집고있을 경우)
            if (thisGrabber != null)
            {
                input.VibrateController(0.1f, 0.2f, 0.1f, thisGrabber.HandSide);
            }

            if (isDrill)
            {
                drill.OnSpin();
            }


                // 사격이 가능할 때 실행. 발사체 또는 레이로 분류
            bool useProjectile = AlwaysFireProjectile || (FireProjectileInSlowMo && Time.timeScale < 1);
            if (useProjectile)
            {
                GameObject projectile = Instantiate(ProjectilePrefab, MuzzlePointTransform.position, MuzzlePointTransform.rotation) as GameObject;
                Rigidbody projectileRigid = projectile.GetComponentInChildren<Rigidbody>();
                projectileRigid.AddForce(MuzzlePointTransform.forward * ShotForce, ForceMode.VelocityChange);

                Projectile proj = projectile.GetComponent<Projectile>();
                // Convert back to raycast if Time reverts
                if (proj && !AlwaysFireProjectile)
                {
                    proj.MarkAsRaycastBullet();
                }

                // Make sure we clean up this projectile
                Destroy(projectile, 20);
            }
            else
            {
                // Raycast to hit
                RaycastHit hit;
                if (Physics.Raycast(MuzzlePointTransform.position, MuzzlePointTransform.forward, out hit, MaxRange, ValidLayers, QueryTriggerInteraction.Ignore))
                {
                    Debug.DrawRay(MuzzlePointTransform.position, MuzzlePointTransform.forward * MaxRange, Color.red);
                    OnRaycastHit(hit);
                }
            }

            // Apply recoil
            ApplyRecoil();

            // We just fired this bullet
            BulletInChamber = false;

            // Try to load a new bullet into chamber         
            if (AutoChamberRounds)
            {
                chamberRound();
            }
            else
            {
                EmptyBulletInChamber = true;
            }

            // Unable to chamber bullet, force slide back
            if (!BulletInChamber)
            {
                // Do we need to force back the receiver?
                slideForcedBack = ForceSlideBackOnLastShot;

                if (slideForcedBack && ws != null)
                {
                    ws.LockBack();
                }
            }

            // Call Shoot Event
            if (onShootEvent != null)
            {
                onShootEvent.Invoke();
            }

            // Store our last shot time to be used for rate of fire
            lastShotTime = Time.time;

            // Stop previous routine
            if (shotRoutine != null)
            {
                MuzzleFlashObject.SetActive(false);
                StopCoroutine(shotRoutine);
            }

            if (AutoChamberRounds)
            {
                shotRoutine = animateSlideAndEject();
                StartCoroutine(shotRoutine);
            }
            else
            {
                shotRoutine = doMuzzleFlash();
                StartCoroutine(shotRoutine);
            }
        }

        // Apply recoil by requesting sprinyness and apply a local force to the muzzle point
        public virtual void ApplyRecoil()
        {
            if (weaponRigid != null && RecoilForce != Vector3.zero)
            {

                // Make weapon springy for X seconds
                grab.RequestSpringTime(RecoilDuration);

                // Apply the Recoil Force
                weaponRigid.AddForceAtPosition(MuzzlePointTransform.TransformDirection(RecoilForce), MuzzlePointTransform.position, ForceMode.VelocityChange);
            }
        }

        // Hit something without Raycast. Apply damage, apply FX, etc.
        public virtual void OnRaycastHit(RaycastHit hit)
        {

            ApplyParticleFX(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit.collider);

            // push object if rigidbody
            Rigidbody hitRigid = hit.collider.attachedRigidbody;
            if (hitRigid != null)
            {
                hitRigid.AddForceAtPosition(BulletImpactForce * MuzzlePointTransform.forward, hit.point);
            }

            // Damage if possible
            Damageable d = hit.collider.GetComponent<Damageable>();
            if (d)
            {
                d.DealDamage(Damage, hit.point, hit.normal, true, gameObject, hit.collider.gameObject);

                if (onDealtDamageEvent != null)
                {
                    onDealtDamageEvent.Invoke(Damage);
                }
            }

            // Call event
            if (onRaycastHitEvent != null)
            {
                onRaycastHitEvent.Invoke(hit);
            }
        }

        public virtual void ApplyParticleFX(Vector3 position, Quaternion rotation, Collider attachTo)
        {
            if (HitFXPrefab)
            {
                GameObject impact = Instantiate(HitFXPrefab, position, rotation) as GameObject;

                // Attach bullet hole to object if possible
                BulletHole hole = impact.GetComponent<BulletHole>();
                if (hole)
                {
                    hole.TryAttachTo(attachTo);
                }
            }
        }

        /// <summary>
        /// Something attached ammo to us
        /// </summary>
        public virtual void OnAttachedAmmo()
        {

            // May have ammo loaded
            updateChamberedBullet();

            if (onAttachedAmmoEvent != null)
            {
                onAttachedAmmoEvent.Invoke();
            }
        }

        // Ammo was detached from the weapon
        public virtual void OnDetachedAmmo()
        {
            // May have ammo loaded / unloaded
            updateChamberedBullet();

            if (onDetachedAmmoEvent != null)
            {
                onDetachedAmmoEvent.Invoke();
            }
        }

        public virtual int GetBulletCount()
        {
            if (ReloadMethod == ReloadType.InfiniteAmmo)
            {
                return 9999;
            }
            else if (ReloadMethod == ReloadType.InternalAmmo)
            {
                return (int)InternalAmmo;
            }
            else if (ReloadMethod == ReloadType.ManualClip)
            {
                return GetComponentsInChildren<Bullet>(false).Length;
            }

            // Default to bullet count
            return GetComponentsInChildren<Bullet>(false).Length;
        }

        public virtual void RemoveBullet()
        {

            // Don't remove bullet here
            if (ReloadMethod == ReloadType.InfiniteAmmo)
            {
                return;
            }

            else if (ReloadMethod == ReloadType.InternalAmmo)
            {
                InternalAmmo--;
            }
            else if (ReloadMethod == ReloadType.ManualClip)
            {
                Bullet firstB = GetComponentInChildren<Bullet>(false);
                // Deactivate gameobject as this bullet has been consumed
                if (firstB != null)
                {
                    Destroy(firstB.gameObject);
                }
            }

            // Whenever we remove a bullet is a good time to check the chamber
            updateChamberedBullet();
        }


        public virtual void Reload()
        {
            InternalAmmo = MaxInternalAmmo;
        }

        void updateChamberedBullet()
        {
            if (ChamberedBullet != null)
            {
                ChamberedBullet.gameObject.SetActive(BulletInChamber || EmptyBulletInChamber);
            }
        }

        void chamberRound()
        {

            int currentBulletCount = GetBulletCount();

            if (currentBulletCount > 0)
            {
                // Remove the first bullet we find in the clip                
                RemoveBullet();

                // That bullet is now in chamber
                BulletInChamber = true;
            }
            // Unable to chamber a bullet
            else
            {
                BulletInChamber = false;
            }
        }

        protected IEnumerator shotRoutine;

        // Randomly scale / rotate to make them seem different
        void randomizeMuzzleFlashScaleRotation()
        {
            MuzzleFlashObject.transform.localScale = Vector3.one * Random.Range(0.75f, 1.5f);
            MuzzleFlashObject.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, 90f));
        }

        public virtual void OnWeaponCharged(bool allowCasingEject)
        {

            // Already bullet in chamber, eject it
            if (BulletInChamber && allowCasingEject)
            {
                ejectCasing();
            }
            else if (EmptyBulletInChamber && allowCasingEject)
            {
                ejectCasing();
                EmptyBulletInChamber = false;
            }

            chamberRound();

            // Slide is no longer forced back if weapon was just charged
            slideForcedBack = false;

            if (onWeaponChargedEvent != null)
            {
                onWeaponChargedEvent.Invoke();
            }
        }

        protected virtual void ejectCasing()
        {
            if (isDrill)
                return;
            GameObject shell = Instantiate(BulletCasingPrefab, EjectPointTransform.position, EjectPointTransform.rotation) as GameObject;
            Rigidbody rb = shell.GetComponentInChildren<Rigidbody>();

            if (rb)
            {
                rb.AddRelativeForce(Vector3.right * BulletCasingForce, ForceMode.VelocityChange);
            }

            // Clean up shells
            GameObject.Destroy(shell, 5);
        }

        protected virtual IEnumerator doMuzzleFlash()
        {
            MuzzleFlashObject.SetActive(true);
            yield return new WaitForSeconds(0.05f);

            randomizeMuzzleFlashScaleRotation();
            yield return new WaitForSeconds(0.05f);

            MuzzleFlashObject.SetActive(false);
        }

        // Animate the slide back, eject casing, pull slide back
        protected virtual IEnumerator animateSlideAndEject()
        {

            // Start Muzzle Flash
            MuzzleFlashObject.SetActive(true);

            int frames = 0;
            bool slideEndReached = false;
            Vector3 slideDestination = new Vector3(0, 0, SlideDistance);

            if (SlideTransform)
            {
                while (!slideEndReached)
                {


                    SlideTransform.localPosition = Vector3.MoveTowards(SlideTransform.localPosition, slideDestination, Time.deltaTime * slideSpeed);
                    float distance = Vector3.Distance(SlideTransform.localPosition, slideDestination);

                    if (distance <= minSlideDistance)
                    {
                        slideEndReached = true;
                    }

                    frames++;

                    // Go ahead and update muzzleflash in sync with slide
                    if (frames < 2)
                    {
                        randomizeMuzzleFlashScaleRotation();
                    }
                    else
                    {
                        slideEndReached = true;
                        MuzzleFlashObject.SetActive(false);
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                yield return new WaitForEndOfFrame();
                randomizeMuzzleFlashScaleRotation();
                yield return new WaitForEndOfFrame();

                MuzzleFlashObject.SetActive(false);
                slideEndReached = true;
            }

            // Set Slide Position
            if (SlideTransform)
            {
                SlideTransform.localPosition = slideDestination;
            }

            yield return new WaitForEndOfFrame();
            MuzzleFlashObject.SetActive(false);

            // Eject Shell
            ejectCasing();

            // Pause for shell to eject before returning slide
            yield return new WaitForEndOfFrame();


            if (!slideForcedBack && SlideTransform != null)
            {
                // Slide back to original position
                frames = 0;
                bool slideBeginningReached = false;
                while (!slideBeginningReached)
                {

                    SlideTransform.localPosition = Vector3.MoveTowards(SlideTransform.localPosition, Vector3.zero, Time.deltaTime * slideSpeed);
                    float distance = Vector3.Distance(SlideTransform.localPosition, Vector3.zero);

                    if (distance <= minSlideDistance)
                    {
                        slideBeginningReached = true;
                    }

                    if (frames > 2)
                    {
                        slideBeginningReached = true;
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    //public enum FiringType
    //{
    //    Semi,
    //    Automatic
    //}

    //public enum ReloadType
    //{
    //    InfiniteAmmo,
    //    ManualClip,
    //    InternalAmmo
    //}
}

