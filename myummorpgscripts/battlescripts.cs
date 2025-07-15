//lots of scripts fo handeling features in ummorpg.  this is still a work in progress.  would like to thank trugord for implementing many of these features already into his arsenal.  
//hopefully someone will find use for these ideas.
using Controller2k;
using Mirror;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GameZero;
using UMA.CharacterSystem;
using UMA;
using System.Linq;
using SQLite;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;



namespace uMMORPG
{

    public enum EquipmentSlotLocations
    {
        None = 0,
        Helmet = 1,
        Glasses = 2,
        Cigar = 3,
        Mask = 4,
        Necklace = 5,
        Cape = 6,
        Wings = 7,

        Shoulders = 8,
        Tabard = 9,
        Chest = 10,
        Tie = 11,
        Shirt = 12,
        Wrist = 13,
        Empty = 14,
        Gloves = 15,
        Waist = 16,
        Tail = 17,
        Legs = 18,
        Feet = 19,
        Ring1 = 20,
        Ring2 = 21,
        MainHand = 22,
        OffHand = 23,
        Trinket1 = 24,
        Trinket2 = 25,

        Artifact = 26,
        Ammo = 27

    }
    public enum HOLSTER_STATE : byte
    {
        SHEATHED,
        DRAWN
    }


    


    public class battlescripts : NetworkBehaviour
    {
        private Coroutine emoteCoroutine;
        bool isDrawn = true;
        public AvatarDefinition avatarDef;
        public string currentDnaRecipe;
        // Add these fields to hold the UMA style recipes
        public UI_BarberShop barber;
        //public PlayerAddonsConfigurator playerAddonsConfigurator;
        //[SyncVar] public float hairR, hairG, hairB, hairA;
        //[SyncVar] public float skinR, skinG, skinB, skinA;
        //[SyncVar] public float eyeR, eyeG, eyeB, eyeA;
        [SyncVar] public string hairColor;
        [SyncVar] public string skinColor;
        [SyncVar] public string eyeColor;
        [SyncVar] public int hairIndex;
        [SyncVar] public int beardIndex;
        [SyncVar] public int eyebrowIndex;
        [SyncVar] public int tattooIndex;
        public DynamicCharacterAvatar dca;
        public Instance instanceTemplate;
        public static int partyId = -1;
        //[SyncVar] public int partyIdToJoin = -1;
        private float castingSkillDuration = 0f;
        private float currentCastTime = 0f;

        //DEL
        [SerializeField] bool allowSolo;

        public Skills skills;
        public bool WearingWings = false;
        public bool inWater => waterCollider != null;
        public GameObject mainhand;
        public GameObject offhand;

        //public HandsRequired hands;
        private bool IsSwappingWeapon = false;
        [SyncVar(hook = nameof(OnHolsterChanged))] public HOLSTER_STATE CURRENT_HOLSTER_STATE = HOLSTER_STATE.SHEATHED;
        public bool isInitial = false;
        public List<Transform> HolsterLocations;

        private Transform GetMainHandWeaponLocation => pe.slotInfo[(int)EquipmentSlotLocations.MainHand].location;
        private Transform GetOffHandWeaponLocation => pe.slotInfo[(int)EquipmentSlotLocations.OffHand].location;
        private bool _hasMainHandWeapon => pe.slots[(int)EquipmentSlotLocations.MainHand].amount > 0;
        private bool _hasOffHandWeapon => pe.slots.Count > (int)EquipmentSlotLocations.OffHand ? pe.slots[(int)EquipmentSlotLocations.OffHand].amount > 0 : false;
        private Vector3 lastMoveDir;
        private bool isSwimming;




        private bool combatActive;
        //public Hands hands; // stores the handedness of THIS weapon
        //public WeaponType weaponType; // stores the type of THIS weapon

        //public List<Transform> HolsterLocations;
        //public enum HandsRequired { ONE_HANDED, TWO_HANDED, NONE } // declares the possible handednesses of weapons

        //public enum HolsterLocation : byte { ONE_HANDED_MAIN, ONE_HANDED_OFF, TWO_HANDED_MAIN, TWO_HANDED_OFF, UPPER_BACK, RANGED }


        public bool AllowedWeaponDrawState => pccm.state != MoveState.DEAD && pccm.state != MoveState.MOUNTED; // add other states if needed.



        public Skill skill;
        public bool ANGEL;
        float runSpeed = 8;
        float stepCycle;
        float nextStep;

        Collider waterCollider;
        bool jumpKeyPressed;


        //public PlayerMountControl mountcontrol;
        public PlayerEquipment pe;




        public LinearInt damage;
        public LinearInt stunChance; // range [0,1]
        public LinearInt stunTime; // in seconds
        public GameObject wingObject;
        public GameObject capeObject;
        public GameObject weaponObject;
        //public NpcCostume costume;

        [Header("Physics")]
        [Tooltip("Apply a small default downward force while grounded in order to stick on the ground and on rounded surfaces. Otherwise walking on rounded surfaces would be detected as falls, preventing the player from jumping.")]
        public float gravityMultiplier = 2;
        public PlayerMountControl mountControl;


        public bool charging;
        private float sitting = 0f;
        private bool running;
        //holster stuff
        //public List<Transform> HolsterLocations;

        public JetpacksNWings jet;




        //private Transform GetMainHandWeaponLocation => pe.slotInfo[(int)EquipmentSlotLocations.MainHand].location;
        // private Transform GetOffHandWeaponLocation => pe.slotInfo[(int)EquipmentSlotLocations.OffHand].location;

        //public enum WeaponType { Unarmed, Sword, Axe, Mace, Fist, Spear, Dagger, Shield, Staff, Bow, Wand, Gun, Tome, None } // declares the possible weapon types
        //[SyncVar(hook = nameof(OnHolsterChanged))] public HOLSTER_STATE CURRENT_HOLSTER_STATE = HOLSTER_STATE.SHEATHED;

        //public static int partyId = -1; // initialize to -1 to indicate that it is not set yet
        //public Player creator;
        private Vector3 targetPos;
        public static battlescripts singleton;

        [Header("charge")]
        public float stopDistance;
        public Vector3 goal;
        public string buffName;
        public Buff buff;

        public PlayerCharacterControllerMovement pccm;

        private Vector3 moveDirection = Vector3.zero;
        public CharacterController2k controller;

        public Player player;
        public Animator animator;
        // public string mainHandidleAni;
        // public string offHandidleAni;
        [Header("battlestances")]
        public bool rootmotion;
        public bool twoMainHandswordAIMnoOH;
        public bool twoOffHandswordAIMnoOH;
        public bool twoMainHandspearAIMnoOH;
        public bool twoMainHandmaceAIMnoOH;
        public bool twoOffHandmaceAIMnoOH;
        public bool twoMainHandaxeAIMnoOH;
        public bool twoOffHandaxeAIMnoOH;
        public bool twoMainHandgunAIMnoOH;
        public bool twoOffHandgunAIMnoOH;
        public bool twoMainHandbowAIMnoOH;
        public bool unarmedAIM;

        public GameObject twohandmainhandsheath;
        public GameObject onehandmainhandsheath;
        public GameObject twohandoffhandsheath;
        public GameObject onehandoffhandsheath;
        public GameObject shieldsheath;
        public GameObject staffsheath;
        [SyncVar]
        public bool weaponDrawnOnLogout = false;


        private MethodInfo eventDiedMethod;
        private MethodInfo eventUnderWaterMethod;

        private Energy energy;



        private Transform casterTransform;
        [Header("mousemanager")]
        public LayerMask raycastBlockingLayers;
        public Transform content;
        public bool isAOE;
        public int storedSkillIndex;
        public bool cursorSelect;

        [SyncVar] public int currentItemIndex;
        [SyncVar] public int skillIndex;
        [SyncVar] public bool isTargeting;
        [SyncVar] public Vector3 storedMousePosition;

        public Texture2D defaultCursor;
        public Texture2D targetcursor;
        public Texture2D attackCursor;
        private CursorMode cursorMode = CursorMode.ForceSoftware;
        private Vector2 hotSpot = Vector2.zero;
        private Vector2 defualtSpot = new Vector2(18f, 5f);
        private Vector2 targetSpot = new Vector2(30f, 30f);
        public Vector2 attackSpot = new Vector2(30f, 30f);

        public GameObject aoeCanvas;
        public Projector targetIndicator;

        //public LayerMask layerMask = 1 << 16;

        [SyncVar] public Vector3 mouseHitPosition;
        public Vector3 posUp;
        public float maxAbility2Distance;

        // public TargetDamageSkill dmgskill;

        //buffs
        //
        //public RotatingStuff rotatingstuff;

        public float HorizontalSpeed => horizontalSpeed;
        [SyncVar]
        public string savedUMADna = "";




        [Header("Crouching")]
        private FieldInfo moveDirField;
        private FieldInfo desiredDirField;
        private FieldInfo inputDirField;
        private FieldInfo waterColliderField;

        private float swimSpeed = 5f;  // Adjust accordingly
        private float horizontalSpeed = 3f;
        private float swimSurfaceOffset = 0.3f;
        [SerializeField] private GameObject playerObject;
        [SerializeField] private Breath breath;
        private bool appearanceLoaded = false;
        private bool isUMACreated = false;
        public ItemSlot[] slots;





        public override void OnStartServer()
        {
            
            Debug.Log("DB path: " + Application.dataPath);
            base.OnStartServer();
            Debug.Log("onstartserver");
            Database.singleton.CreateAppearanceTable();
            Debug.Log("Appearance table created.");


            object result = slots.Count(); // <-- remove parentheses if Count is a property

            Debug.Log($"[Server] OnStartServer: Initialized appearances, slots count: {result}");

            foreach (var slot in slots)
            {
                if (slot.amount > 0)
                    Debug.Log($"Slot item: {slot.item.name} amount: {slot.amount}");
            }


        }

        public void equipmentchanger(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
        {
            // at first, check if the item model actually changed. we don't need to
            // refresh anything if only the durability changed.
            // => this fixes a bug where attack animations were constantly being
            //    reset for no obvious reason. this happened because with durability
            //    items, any time we get attacked the equipment durability changes.
            //    this causes the OnEquipmentChanged hook to be fired, which would
            //    then refresh the location and rebind the animator, causing the
            //    animator to start at the entry state again (hence restart the
            //    animation).
            //
            // note: checking .data is enough. we don't need to check as deep as
            //       .data.model. this way we avoid the EquipmentItem cast.
            ScriptableItem oldItem = oldSlot.amount > 0 ? oldSlot.item.data : null;
            ScriptableItem newItem = newSlot.amount > 0 ? newSlot.item.data : null;
            if (oldItem != newItem)
            {
                Debug.Log($"[equipmentchanger] oldItem: {(oldItem != null ? oldItem.name : "null")}, newItem: {(newItem != null ? newItem.name : "null")}");
                // update the model
                RefreshLocation(index);
                RefreshUMAEquipmentVisuals();
                //RefreshLoadPreviewVisuals();
            }
        }




        public override void OnStartLocalPlayer()
        {
            CloseAll();
            Debug.Log("🧵 Saving costume equipment & inventory after customization...");
            Database.singleton.LoadCostumeEquipment((PlayerEquipment)player.equipment);
            Database.singleton.LoadCostumeInventory(player.inventory);
            base.OnStartLocalPlayer();
            CmdRequestAppearance();
            //GetComponent<PlayerAppearances>().CmdRequestInitialize();
            StartCoroutine(ActivatePreviewerWithDelay(2f));

            Debug.Log("InitializeDefaultAppearances on start server");



            // Load appearance from DB early, assign to bs variables
            Debug.Log("onstartlocalplayer");
            //CmdRefreshUMAPreviewEquipmentVisuals();
            //RefreshLoadPreviewVisuals();
            //StartCoroutine(ApplyAppearanceWhenReady());
            CURRENT_HOLSTER_STATE = HOLSTER_STATE.DRAWN;
            HandleHolsterLogic();
            base.OnStartLocalPlayer();
            //RefreshEquipmentSlots();
            //RefreshPreviewEquipmentVisuals();
            //player.pa.CmdRequestInitialize();

            //DelayedApplyAppearance();

            //CmdRefreshEquipmentSlots();
            //RefreshUMAPreviewEquipmentVisuals();
            //dca.LoadAvatarDefinition(avatarDef);
            //dca.ForceUpdate(true, true, true);
            StartCoroutine(ForceCombatVisualRefresh());
            

            //dca.BuildCharacter(true);
            HandleCapeAnimation();
        }
        
        IEnumerator ForceCombatVisualRefresh()
        {
            // Wait for slots to be valid (esp. 22 and 23)
            yield return new WaitUntil(() =>
                player != null &&
                player.equipment != null &&
                player.equipment.slots.Count > 23
            );

            // Make sure items are loaded
            yield return new WaitForSeconds(0.2f); // tiny buffer

            Debug.Log("🛠 Forcing combat visual refresh on spawn");

            // Manually apply the visual refresh without animation trigger
            ToggleCombat(combatMode, false);
        }


        void RotateWithKeys()
        {

            float horizontal2 = Input.GetAxis("Horizontal2");
            transform.Rotate(Vector3.up * horizontal2 * pccm.rotationSpeed * Time.fixedDeltaTime);

        }



        void PlayLandingSound()
        {
            pccm.feetAudio.clip = pccm.landSound;
            pccm.feetAudio.Play();
            nextStep = stepCycle + .5f;
        }

        void PlayFootStepAudio()
        {
            if (!controller.isGrounded) return;

            // do we have any footstep sounds?
            if (pccm.footstepSounds.Length > 0)
            {
                // pick & play a random footstep sound from the array,
                // excluding sound at index 0

                pccm.feetAudio.PlayOneShot(pccm.feetAudio.clip);

                // move picked sound to index 0 so it's not picked next time

                pccm.footstepSounds[0] = pccm.feetAudio.clip;
            }
        }
        void ProgressStepCycle(Vector3 inputDir, float speed)
        {
            if (controller.velocity.sqrMagnitude > 0 && (inputDir.x != 0 || inputDir.y != 0))
            {
                stepCycle += (controller.velocity.magnitude + (speed * pccm.runStepLength)) * Time.fixedDeltaTime;
            }

            if (stepCycle > nextStep)
            {
                nextStep = stepCycle + pccm.runStepInterval;
                PlayFootStepAudio();
            }
        }






        public interface IToolTipProvider
        {
            string ToolTip(int skillLevel, bool showRequirements = false);
        }

        public string ToolTip(int skillLevel, bool showRequirements = false)
        {
            StringBuilder tip = new StringBuilder();
            tip.Replace("{DAMAGE}", damage.Get(skillLevel).ToString());
            tip.Replace("{STUNCHANCE}", stunChance.Get(skillLevel).ToString());
            tip.Replace("{STUNTIME}", stunTime.Get(skillLevel).ToString());
            return tip.ToString();
        }


        void OnChatSubmitted(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Check if the text is an emote command
            if (text.StartsWith("/sit") || text.StartsWith("/clap") || text.StartsWith("/yell") || text.StartsWith("/sleep"))
            {
                CmdPlayEmote(text);
            }
        }
        void Start()
        {

            var playerChat = GetComponent<PlayerChat>();
            if (playerChat != null)
            {
                playerChat.onSubmit.AddListener(OnChatSubmitted);
            }
            Debug.Log("start called");



            Cursor.SetCursor(defaultCursor, defualtSpot, cursorMode);
            setDefaultCursor();

            if (player.isLocalPlayer)
            {
                targetIndicator.enabled = false;
            }






            OpenAll();


        }




        void Awake()
        {

            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            Debug.Log("AudioListeners count: " + listeners.Length);

            foreach (AudioListener listener in listeners)
            {
                Debug.Log("AudioListener found on: " + listener.gameObject.name);
            }


            HolsterLocations.Add(twohandmainhandsheath.transform);
            HolsterLocations.Add(onehandmainhandsheath.transform);
            HolsterLocations.Add(twohandoffhandsheath.transform);
            HolsterLocations.Add(onehandoffhandsheath.transform);
            HolsterLocations.Add(staffsheath.transform);
            HolsterLocations.Add(shieldsheath.transform);




            // initialize singleton
            if (singleton == null) singleton = this;

        }

        float ApplyGravity(float moveDirY)
        {
            // apply full gravity while falling
            if (!controller.isGrounded)
                // gravity needs to be * Time.fixedDeltaTime even though we multiply
                // the final controller.Move * Time.fixedDeltaTime too, because the
                // unit is 9.81m/s²
                return moveDirY + Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            // if grounded then apply no force. the new OpenCharacterController
            // doesn't need a ground stick force. it would only make the character
            // slide on all uneven surfaces.
            return 0;
        }
        //MOVESTATES####################################################################################################################


        ///charge attack


        public void DashMovement(string buffName, float stopDistance)
        {
            if (player.target != null)
            {
                Vector3 goal = player.target.transform.position;
                int index = player.skills.GetBuffIndexByName(buffName);

                charging = true;
                animator.SetBool("chargeattack", true);
                player.transform.position = Vector3.MoveTowards(player.transform.position, goal, player.speed * Time.fixedDeltaTime);
                player.transform.LookAt(goal);

                if (Vector3.Distance(player.transform.position, goal) <= stopDistance)
                {
                    if (index != -1)
                    {
                        player.skills.buffs.RemoveAt(index);
                    }
                    charging = false;
                    animator.SetBool("chargeattack", false);

                }
            }
        }
        //#########################################################################################################################
        //skill stuff








        //Stance stuff
        //#########################################################################################################################
        public void clearstances()
        {
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetBool("twoMainHandswordAIMnoOH", false);
                anim.SetBool("twoOffHandswordAIMnoOH", false);
                anim.SetBool("oneMainHandswordAIMnoOH", false);
                anim.SetBool("oneOffHandswordAIMnoOH", false);
                anim.SetBool("twoMainHandspearAIMnoOH", false);
                anim.SetBool("twoOffHandspearAIMnoOH", false);
                anim.SetBool("twoMainHandmaceAIMnoOH", false);
                anim.SetBool("twoOffHandmaceAIMnoOH", false);
                anim.SetBool("twoMainHandaxeAIMnoOH", false);
                anim.SetBool("twoOffHandaxeAIMnoOH", false);
                anim.SetBool("twoMainHandgunAIMnoOH", false);
                anim.SetBool("twoOffHandgunAIMnoOH", false);
                anim.SetBool("twoMainHandbowAIMnoOH", false);
                anim.SetBool("AIMING", false);
            }
        }



        public void OpenAll()
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("OpenLeft", true);
                animator.SetBool("OpenRight", true);
            }
        }

        public void CloseLeft()
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("closeLeft", true);
            }
        }

        public void CloseRight()
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("closeRight", true);
            }
        }

        public void OpenLeft()
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("OpenLeft", true);
            }
        }

        public void OpenRight()
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {

                animator.SetBool("OpenRight", true);
            }
        }

        public void MonsterSelected(Entity monster)
        {
            // Use the monster's attributes to determine its category
            string category = GetMonsterCategory(monster);



            // Implement logic based on the selected monster's category
            HandleMonsterBasedOnCategory(category);
        }

        private string GetMonsterCategory(Entity monster)
        {
            // Determine the category based on monster attributes
            if (monster.isBoss)
            {
                return "Boss";
            }
            else if (monster.isElite)
            {
                return "Elite";
            }
            else
            {
                return "Normal"; // Default category
            }
        }

        private void HandleMonsterBasedOnCategory(string category)
        {
            // Implement your logic based on the monster's category
            // For example:
            switch (category)
            {
                case "Boss":
                    // Handle Boss logic
                    Debug.Log("Handling Boss Monster");
                    break;
                case "Elite":
                    // Handle Elite logic
                    Debug.Log("Handling Elite Monster");
                    break;
                case "Normal":
                    // Handle Normal logic
                    //Debug.Log("Handling Normal Monster");
                    break;
                default:
                    Debug.LogWarning("Unknown monster category");
                    break;
            }
        }














        // Mapping custom states to original states
        /*public MoveState MapToOriginalState(MoveState customState)
        {
            switch (customState)
            {
                case MoveState.ANGELMOUNTED:
                    return MoveState.ANGELMOUNTED; // Map to the closest original state
                case MoveState.CROUCHING:
                    return MoveState.CROUCHING; // This is an example; adjust as needed
                case MoveState.SNEAKING:
                    return MoveState.SNEAKING;
                case MoveState.WALKING:
                    return MoveState.WALKING;// Example mapping (adjust as needed)
                case MoveState.CRAWLING:
                    return MoveState.CRAWLING; // Example mapping
                case MoveState.CLIMBING:
                    return MoveState.CLIMBING; // No change needed
                default:
                    return MoveState.IDLE; // Default fallback
            }
            
    }*/
        [ClientCallback]
        void UpdateAnimations()
        {

            // Apply animation parameters to all animators
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {

                animator.SetBool("CROUCHING", pccm.state == MoveState.CROUCHING);
                animator.SetBool("CRAWLING", pccm.state == MoveState.CRAWLING);
                animator.SetBool("CLIMBING", pccm.state == MoveState.CLIMBING);
                animator.SetBool("SNEAKING", pccm.state == MoveState.SNEAKING);

                if (pccm.state != MoveState.CLIMBING)
                {
                    int weaponIndex = pe.GetEquippedWeaponIndex();
                    if (weaponIndex != -1)
                    {

                        WeaponItem weapon = (WeaponItem)pe.slots[weaponIndex].item.dataCostume;

                    }




                }

            }
        }

       

        [Command]
        void CmdPlayEmote(string command)
        {
            RpcPlayEmote(command);
        }

        [ClientRpc]
        void RpcPlayEmote(string command)
        {
            var anim = GetComponent<Animator>();
            if (anim == null)
            {
                Debug.LogWarning("Animator not found on player");
                return;
            }



            if (command.StartsWith("/sleep"))
            {
                
                anim.SetTrigger("sleep"); // looping emote
            }
            else if (command.StartsWith("/sit"))
            {
                
                anim.SetTrigger("sit");
            }
            else if (command.StartsWith("/clap"))
            {
                
                anim.SetTrigger("clap");
            }
            else if (command.StartsWith("/yell"))
            {
                
                anim.SetTrigger("yell");
            }
            else if (command.StartsWith("/headspin"))
            {
               
                anim.SetTrigger("headspin");
            }
            else if (command.StartsWith("/cheer"))
            {
                
                anim.SetTrigger("cheer");
            }
            else if (command.StartsWith("/salute"))
            {
                
                anim.SetTrigger("salute");
            }
            else if (command.StartsWith("/wave"))
            {
                    anim.SetTrigger("wave");
            }
            else if (command.StartsWith("/cry"))
            {
                
                anim.SetTrigger("cry");
            }
            else if (command.StartsWith("/point"))
            {
                
                anim.SetTrigger("point");
            }
            else if (command.StartsWith("/bow"))
            {
                
                anim.SetTrigger("bow");
            }
            else if (command.StartsWith("/kneel"))
            {
               
                anim.SetTrigger("kneel");
            }
            else if (command.StartsWith("/breakdance"))
            {
                
                anim.SetTrigger("breakdance");
            }
            if (command.StartsWith("/dance"))
            {

                anim.SetTrigger("dance"); // looping emote
            }
            if (command.StartsWith("/taunt"))
            {

                anim.SetTrigger("taunt"); // looping emote
            }
            if (command.StartsWith("/flex"))
            {

                anim.SetTrigger("flex"); // looping emote
            }
        }

        bool HandleEmoteCommand(string text)
        {
            string[] validEmotes = new string[]
            {
            "/sit", "/sleep", "/clap", "/wave", "/cry", "/cheer",
            "/salute", "/headspin", "/yell", "/point", "/bow", "/kneel", "/breakdance", "/dance", "/taunt"
            };

            foreach (string emote in validEmotes)
            {
                if (text.StartsWith(emote))
                {
                    CmdPlayEmote(text);
                    return true;
                }
            }
            return false;
        }

        private void FixedUpdate()
        {
            UIChat chat = FindFirstObjectByType<UIChat>();
            if (!NetworkClient.active || Player.localPlayer == null) return;

            // Check if Enter/Return was pressed while chat field is focused
            if (Input.GetKeyDown(KeyCode.Return) && chat.messageInput.isFocused)
            {
                string text = chat.messageInput.text.Trim();

                if (!string.IsNullOrEmpty(text))
                {
                    if (HandleEmoteCommand(text))
                    {
                        // Emote was triggered
                        chat.messageInput.text = "";
                        chat.messageInput.DeactivateInputField(); // optional
                    }
                    else
                    {
                        // Not an emote, send to normal chat
                        Player.localPlayer.chat.OnSubmit(text);
                        chat.messageInput.text = "";
                    }
                }
            }
        

        


            Vector2 inputDir = player.IsMovementAllowed() ? pccm.GetInputDirection() : Vector2.zero;
            Vector3 desiredDir = pccm.GetDesiredDirection(inputDir);

            if (pccm.state == MoveState.SWIMMING)
            {
                Debug.Log("pccm.state=swimmng");
                pccm.AdjustSwimming(inputDir, desiredDir);
            }
            if (pccm.state == MoveState.MOUNTED_SWIMMING)
            {
                Debug.Log("pccm.state=mounted swimming");
                pccm.AdjustMountedSwimming(inputDir, desiredDir);
            }

            if (pccm.state == MoveState.IDLE) pccm.state = pccm.UpdateIDLEextended(inputDir, desiredDir);
            else if (pccm.state == MoveState.RUNNING) pccm.state = pccm.UpdateRUNNINGextended(inputDir, desiredDir);
            else if (pccm.state == MoveState.AIRBORNE) pccm.state = pccm.UpdateAIRBORNEextended(inputDir, desiredDir);
            else if (pccm.state == MoveState.MOUNTED_AIRBORNE) pccm.state = pccm.UpdateMOUNTED_AIRBORNEextended(inputDir, desiredDir);
            else if (pccm.state == MoveState.MOUNTED) pccm.state = pccm.UpdateMOUNTEDextended(inputDir, desiredDir);
            //else if (pccm.state == MoveState.SWIMMING) pccm.state = pccm.UpdateSWIMMINGextended(inputDir, desiredDir);
            //else if (pccm.state == MoveState.MOUNTED_SWIMMING) pccm.state = pccm.UpdateMOUNTED_SWIMMINGextended(inputDir, desiredDir);
        }
        public void LateUpdate()
        {

            jet = wingObject.GetComponentInChildren<JetpacksNWings>();
            HandleCapeAnimation();
            UpdateAnimations();
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {

                anim.SetBool("FLYING", pccm.state == MoveState.AIRBORNE);
                if (anim != null)
                {
                    anim.SetBool("FLYING", pccm.state == MoveState.AIRBORNE);

                    if (anim.runtimeAnimatorController != null)
                    {
                        if (anim.runtimeAnimatorController.name != "fattestcapecontroller" && anim.runtimeAnimatorController.name != "capecontroller")
                        {
                            anim.SetBool("MOUNTED", mountControl.IsMounted());
                        }
                        else
                        {
                            anim.SetBool("ANGELMOUNTED", false);
                            anim.SetBool("MOUNTED", false);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Animator reference is null in LateUpdate");
                }

                if (charging)
                {
                    anim.SetBool("chargeattack", true);
                }
                else if (!charging)
                {
                    anim.SetBool("chargeattack", false);
                }
            }


            if (player.IsMovementAllowed())
            {
                // Toggle crawl state when the crawl key is pressed
                if (Input.GetKeyDown(pccm.crawlKey))
                {
                    Debug.Log("Crawl key pressed");
                    pccm.crawlKeyPressed = !pccm.crawlKeyPressed;
                    if (pccm.crawlKeyPressed)
                    {
                        // If crawling, set the state to crawling and ensure no other conflicting state is active
                        pccm.state = MoveState.CRAWLING;
                        Debug.Log("State changed to CRAWLING");
                    }
                    else
                    {
                        pccm.state = MoveState.IDLE; // Or another default state if not crawling
                        Debug.Log("State changed to IDLE");
                    }
                }

                // Toggle crouch state when the crouch key is pressed
                if (Input.GetKeyDown(pccm.crouchKey))
                {
                    pccm.crouchKeyPressed = !pccm.crouchKeyPressed;
                    if (pccm.crouchKeyPressed)
                    {
                        pccm.state = MoveState.CROUCHING;  // Set state to crouching
                        Debug.Log("State changed to CROUCHING");
                    }
                    else
                    {
                        pccm.state = MoveState.IDLE; // Or another default state if not crouching
                        Debug.Log("State changed to IDLE");
                    }
                }

                // Toggle walking state when the walk key is pressed
                if (Input.GetKeyDown(pccm.walkKey))
                {
                    pccm.walkKeyPressed = !pccm.walkKeyPressed;
                    if (pccm.walkKeyPressed)
                    {
                        pccm.state = MoveState.WALKING;  // Set state to walking
                        Debug.Log("State changed to WALKING");
                    }
                    else
                    {
                        pccm.state = MoveState.IDLE; // Or another default state if not crouching
                        Debug.Log("State changed to IDLE");
                    }
                }

                // Toggle sneaking state when the sneak key is pressed
                if (Input.GetKeyDown(pccm.sneakKey))
                {
                    pccm.sneakKeyPressed = !pccm.sneakKeyPressed;
                    if (pccm.sneakKeyPressed)
                    {
                        pccm.state = MoveState.SNEAKING;  // Set state to sneaking
                        Debug.Log("State changed to SNEAKING");
                    }
                    else
                    {
                        pccm.state = MoveState.IDLE; // Or another default state if not crouching
                        Debug.Log("State changed to IDLE");
                    }
                }
            }




            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetBool("FLYING", pccm.state == MoveState.AIRBORNE && WearingWings);



                if (charging)
                {
                    anim.SetBool("chargeattack", true);
                }
                else if (!charging)
                {
                    anim.SetBool("chargeattack", false);
                }
            }
        }


        public void Update()
        {

            if (controller == null) return;




            if (!isLocalPlayer) return; // Ensure this runs only on the local player

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastBlockingLayers))
            {
                if (hit.collider.gameObject != this.gameObject)
                {
                    posUp = hit.point;

                    // Only update and call CmdSetMousePosition if mouseHitPosition has changed
                    if (Vector3.Distance(mouseHitPosition, hit.point) > 0.01f)
                    {
                        mouseHitPosition = hit.point;
                        CmdSetMousePosition(mouseHitPosition);
                    }
                }
            }

            Quaternion transRot = Quaternion.LookRotation(mouseHitPosition - player.transform.position);

            var hitPosDir = (hit.point - transform.position).normalized;
            float distance = Vector3.Distance(hit.point, transform.position);
            distance = Mathf.Min(distance, maxAbility2Distance);

            var newHitPos = transform.position + hitPosDir * distance;
            aoeCanvas.transform.position = newHitPos;



            if (isLocalPlayer)
            {
                // Handle the specific equipment logic when airborne
                if (Input.GetKeyDown(KeyCode.Z) && !mountControl.activeMount)
                {
                    Debug.Log("Z key pressed, trying to toggle combat.");
                    TryChangeHolsterState();
                    CmdToggleCombat(!combatMode, true);
                    for (int i = 0; i < pe.slots.Count; ++i)
                        if (!pe.slots[i].Equals(default(ItemSlot)))
                        {
                            RefreshLocation(i);
                        }
                        else
                        {
                            Debug.LogWarning("Slot " + i + " is empty or invalid.");
                        }
                }

            }


        }



        //SELECTION
        //###################################################################################################################################################################




        //###################################################################################################################################################################

        //ARENA stuff

        // copy party to all members & save in dictionary

        //[Header("Components")]

        //public int instanceId;
        //static int nextPartyId = 1;
        //static Dictionary<int, SoloParty> parties = new Dictionary<int, SoloParty>();



        [Command(requiresAuthority = false)]
        public void CmdPortPlayerToStartZone()
        {
            // Ensure this logic runs only on the server
            if (!isServer) return;


            if (player != null)
            {
                Debug.Log("Server: Player is not null");

                // Teleport player to start zone
                Vector3 exitPos = new Vector3(-10235, 0, 8220);

                // Assuming player.movement.Warp() is a method to change player position
                player.movement.Warp(exitPos);
                Debug.Log("Server: Player warped to start zone");
            }
            else
            {
                Debug.LogError("Server: Player instance is null");
            }

            // Server-side logic to hide the exit panel, assuming it needs to be controlled by the server

        }


        [Command(requiresAuthority = false)]
        public void CmdLobbyEnd()
        {
            if (partyId != 0)
            {
                // Dismiss the party
                PartySystem.DismissParty(partyId, "System");
                Debug.Log("Instance " + name + " destroyed because no members of party " + partyId + " are in it anymore.");
            }
            else
            {
                Debug.LogWarning("No party to dismiss.");
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdLobbyCreator()
        {
            Debug.Log("Player assigned: " + player.name);
            if (player != null)
            {
                if (!player.party.InParty() && allowSolo)
                    PartySystem.FormParty(player.name, player.name);
                partyId = player.party.party.partyId;
                Debug.Log($"partyId = {partyId}");
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdLobbyJoiner()
        {
            if (player != null)
            {
                PartySystem.AddToParty(partyId, player.name);
            }
        }
        [Command(requiresAuthority = false)]
        public void CmdDungeonSetup(int instanceId)
        {

            //DEL
            if (!player.party.InParty() && allowSolo)
                PartySystem.FormParty(player.name, player.name);
            ArenaManager.singleton.CmdSetSelectedArena(instanceId);
            Instance instanceTemplate = ArenaManager.singleton.GetInstanceTemplate(instanceId);
            // check party again, just to be sure.
            if (player.party.InParty())
            {
                // is there an instance for the player's party yet?
                if (instanceTemplate.instances.TryGetValue(player.party.party.partyId, out Instance existingInstance))
                {
                    // teleport player to instance entry
                    if (player.isServer) player.movement.Warp(existingInstance.entry.position);
                    Debug.Log("Teleporting " + player.name + " to existing instance=" + existingInstance.name + " with partyId=" + player.party.party.partyId);
                }
                // otherwise create a new one
                else
                {
                    Instance instance = Instance.CreateInstance(instanceTemplate, player.party.party.partyId);
                    if (Application.isEditor || Application.isBatchMode)
                    {
                        NetworkServer.Spawn(instance.gameObject);
                    }
                    if (instance != null)
                    {
                        // teleport player to instance entry
                        if (player.isServer) player.movement.Warp(instance.entry.position);
                        Debug.Log("Teleporting " + player.name + " to new instance=" + instance.name + " with partyId=" + player.party.party.partyId);
                    }
                    else if (player.isServer) player.chat.TargetMsgInfo("There are already too many " + instanceTemplate.name + " instances. Please try again later.");
                }
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdPartySetup()
        {
            //ArenaManager am = GameObject.FindWithTag("GameManager").GetComponent<ArenaManager>();
            Instance instanceTemplate = ArenaManager.singleton.GetInstanceTemplate(ArenaManager.singleton.instanceId);

            Debug.Log("Cmdpartysetup called");
            Debug.Log("instanceTemplate = " + instanceTemplate);
            Player[] playersInParty = PartySystem.GetPlayersInParty(partyId);
            Debug.Log("Number of players in party: " + playersInParty.Length);
            foreach (Player player in playersInParty)
            {
                Debug.Log("player name = " + player.name);


                // loop through each player and do something with them
                Debug.Log("player isServer: " + player.isServer + " hasAuthority: " + player.isOwned + " isClient: " + player.isClient + " isLocalPlayer: " + player.isLocalPlayer);


                // collider might be in player's bone structure. look in parents.

                if (player != null)
                {
                    Debug.Log("player is not null");
                    // only call this for server and for local player. not for other
                    // players on the client. no need in locally creating their
                    // instances too.
                    if (player.isServer || player.isLocalPlayer)
                    {
                        Debug.Log("player is server or local player");
                        // required level?
                        if (player.level.current >= instanceTemplate.requiredLevel)
                        {
                            Debug.Log("player met level requirement");
                            // can only enter with a party
                            if (player.party.InParty())
                            {
                                Debug.Log("player is in a party");
                                // is there an instance for the player's party yet?
                                if (instanceTemplate.instances.TryGetValue(partyId, out Instance existingInstance))
                                {
                                    // teleport player to instance entry
                                    if (player.isServer)
                                    {
                                        InstanceId instanceid = existingInstance.GetComponent<InstanceId>();
                                        Debug.Log("player has authority!!");
                                        Vector3 entry1Pos = instanceid.entry1?.position ?? Vector3.zero;
                                        Vector3 entry2Pos = instanceid.entry2?.position ?? Vector3.zero;

                                        // Determine the target position for the player based on the player index

                                        int playerIndex = Array.IndexOf(playersInParty, player);
                                        Vector3 targetPos = (playerIndex % 2 == 0) ? entry2Pos : entry1Pos;

                                        // Call the CmdWarpToEntry() method to move the player to the target position
                                        if (player.isServer || (player.isClient && player.isOwned))
                                        {
                                            // Call the CmdWarpDrive() method only if the player is owned by a client
                                            player.movement.Warp(targetPos);
                                        }
                                        Debug.Log("Teleporting " + player.name + " to existing instance=" + existingInstance.name + " with partyId=" + partyId);
                                    }

                                }
                                // otherwise create a new one
                                else
                                {
                                    Instance instance = Instance.CreateInstance(instanceTemplate, player.party.party.partyId);
                                    Debug.Log("creating instance");
                                    if (Application.isEditor || Application.isBatchMode)
                                    {
                                        NetworkServer.Spawn(instance.gameObject);
                                    }

                                    if (instance != null)
                                    {
                                        Debug.Log("instance is not null");
                                        // teleport player to instance entry
                                        Debug.Log("player isServer: " + player.isServer + " hasAuthority: " + player.isOwned + " isClient: " + player.isClient + " isLocalPlayer: " + player.isLocalPlayer + "isowned" + player.isOwned);

                                        if (player.isServer)
                                        {
                                            InstanceId instanceid = instance.GetComponent<InstanceId>();
                                            Debug.Log("has authority");
                                            // Get the entry positions for the instance
                                            Vector3 entry1Pos = instanceid.entry1?.position ?? Vector3.zero;
                                            Vector3 entry2Pos = instanceid.entry2?.position ?? Vector3.zero;

                                            // Determine the target position for the player based on the player index

                                            int playerIndex = Array.IndexOf(playersInParty, player);
                                            Vector3 targetPos = (playerIndex % 2 == 0) ? entry2Pos : entry1Pos;

                                            // Call the CmdWarpToEntry() method to move the player to the target position
                                            if (player.isServer || (player.isClient && player.isOwned))
                                            {
                                                if (player.GetComponent<NetworkIdentity>().isOwned)
                                                {
                                                    Debug.Log("client has authority for cmdwarpdrive");
                                                    // Check if client is still connected
                                                    if (!NetworkServer.connections.ContainsKey(player.GetComponent<NetworkIdentity>().connectionToClient.connectionId))
                                                    {
                                                        // Client has disconnected, do something
                                                        Debug.Log("client has disconnected, do something!");
                                                    }
                                                    else
                                                    {
                                                        // Call the CmdWarpDrive() method only if the player is owned by a client
                                                        player.movement.Warp(targetPos);
                                                        Debug.Log("player passed all checks, warping successfully");
                                                    }
                                                }
                                                Debug.Log("player didn't have authority, still warping just in case");
                                                player.movement.Warp(targetPos);
                                            }
                                            Debug.Log("Teleporting " + player.name + " to new instance=" + instance.name + " with partyId=" + player.party.party.partyId);
                                        }
                                        else { Debug.Log("player is not server!!"); }

                                    }
                                    else if (player.isServer) player.chat.TargetMsgInfo("There are already too many " + instance.name + " instances. Please try again later.");
                                }
                            }

                            else
                            {
                                Debug.LogError("No existing instance found!");
                            }
                        }
                    }

                }
            }

        }








        //###################################################################################################################################################################
        //AOE ATTACKS 




        public bool isMouseOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }




        public void setTargetCursor()
        {
            Cursor.SetCursor(targetcursor, targetSpot, cursorMode);
            isTargeting = true;
        }

        public void setAttackCursor()
        {
            Cursor.SetCursor(attackCursor, attackSpot, cursorMode);
        }
        public void setLockpickTargetCursor()
        {
            Cursor.SetCursor(targetcursor, targetSpot, cursorMode);
            isTargeting = true;

        }
        [Command]
        public void CmdSetDefaultCursor()
        {
            Cursor.SetCursor(defaultCursor, defualtSpot, cursorMode);

            isTargeting = false;
            currentItemIndex = -1;
            skillIndex = -1;
            targetIndicator.enabled = false;

        }
        [Server]
        public void setDefaultCursor()
        {
            Cursor.SetCursor(defaultCursor, defualtSpot, cursorMode);

            isTargeting = false;
            currentItemIndex = -1;
            skillIndex = -1;
            targetIndicator.enabled = false;
        }
        [Command]
        public void CmdSetTargeting(bool value)
        {
            isTargeting = value;
        }

        [Command]
        public void CmdSetSkillIndex(int index)
        {
            skillIndex = index;
        }

        [Command]
        public void CmdSetCurrentItemIndex(int index)
        {
            currentItemIndex = index;
        }
        private void SetCustomCursor(Texture2D curText)
        {
            Cursor.SetCursor(curText, hotSpot, cursorMode);
        }

        [Command]
        public void CmdSetVector3(Vector3 mousePos)
        {

            storedMousePosition = mousePos;
        }

        [Command]
        public void CmdSetMousePosition(Vector3 position)
        {
            mouseHitPosition = position; // Store the position on the server
        }

        //holsterstuff##############################################################################################################################################
        //from weaponitem




        //from equipment 
        public int GetEquippedWeaponIndex()
        {
            // Avoid FindIndex to minimize allocations.
            for (int i = 0; i < pe.slots.Count; ++i)
            {
                ItemSlot slot = pe.slots[i];
                if (slot.amount > 0 && slot.item.data is WeaponItem)
                    return i;
            }
            return -1;
        }

        // Get equipped weapon category or empty string if none equipped.
        public string GetEquippedWeaponCategory()
        {
            int index = GetEquippedWeaponIndex();
            return index != -1 ? ((WeaponItem)pe.slots[index].item.data).category : "";
        }

        // Get equipped weapon type or default if none.
        public ScriptableItem.WeaponType GetEquippedWeaponType()
        {
            int index = GetEquippedWeaponIndex();
            return index != -1 ? pe.slots[index].item.data.weaponType : ScriptableItem.WeaponType.None;
        }

        public WeaponItem GetMainHandWeapon()
        {
            return _hasMainHandWeapon ? pe.slots[(int)EquipmentSlotLocations.MainHand].item.data as WeaponItem : null;
        }

        public WeaponItem GetOffHandWeapon()
        {
            return _hasOffHandWeapon ? pe.slots[(int)EquipmentSlotLocations.OffHand].item.data as WeaponItem : null;
        }

        public void HandleHolsterLogic()
        {
            if (HolsterLocations == null) return;

            // Disable all holster locations first to prevent leftover enabled objects.
            foreach (Transform location in HolsterLocations)
            {
                if (location != null)
                    location.gameObject.SetActive(false);
            }

            bool weaponDrawn = CURRENT_HOLSTER_STATE == HOLSTER_STATE.DRAWN;

            // Main hand weapon holster logic
            if (_hasMainHandWeapon)
            {
                WeaponItem mainWeapon = GetMainHandWeapon();
                if (mainWeapon != null)
                {
                    int mainHolsterIndex = GetHolsterIndex(mainWeapon, true);
                    if (mainHolsterIndex >= 0 && mainHolsterIndex < HolsterLocations.Count)
                    {
                        HolsterLocations[mainHolsterIndex].gameObject.SetActive(!weaponDrawn);
                        if (GetMainHandWeaponLocation != null && GetMainHandWeaponLocation.childCount > 0)
                            GetMainHandWeaponLocation.GetChild(0).gameObject.SetActive(weaponDrawn);
                    }
                }
            }

            // Off hand weapon holster logic
            if (_hasOffHandWeapon)
            {
                WeaponItem offWeapon = GetOffHandWeapon();
                if (offWeapon != null)
                {
                    int offHolsterIndex = GetHolsterIndex(offWeapon, false);
                    if (offHolsterIndex >= 0 && offHolsterIndex < HolsterLocations.Count)
                    {
                        HolsterLocations[offHolsterIndex].gameObject.SetActive(!weaponDrawn);
                        if (GetOffHandWeaponLocation != null && GetOffHandWeaponLocation.childCount > 0)
                            GetOffHandWeaponLocation.GetChild(0).gameObject.SetActive(weaponDrawn);
                    }
                }
            }
        }

        

        void OnHolsterChanged(HOLSTER_STATE oldState, HOLSTER_STATE newState)
        {
            if (gameObject.activeInHierarchy)
            {
                Debug.Log($"Holster state changed from {oldState} to {newState}");
                StartCoroutine(ChangeWeaponRoutine());
            }
        }

        private IEnumerator ChangeWeaponRoutine()
        {
            IsSwappingWeapon = true;

            Animator[] animators = GetComponentsInChildren<Animator>();
            if (animators != null && animators.Length > 0)
            {
                DrawHolsterAnimationTriggers trigger = EvaluateHolsterAnimation();
                Debug.Log($"Triggering holster animation: {trigger}");
                foreach (Animator anim in animators)
                {
                    anim.SetTrigger(trigger.ToString());
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (HolsterLocations != null)
            {
                foreach (Transform location in HolsterLocations)
                {
                    if (location != null)
                    {
                        location.gameObject.SetActive(false);
                    }
                }
            }

            if (GetMainHandWeaponLocation != null)
                GetMainHandWeaponLocation.gameObject.SetActive(false);

            if (GetOffHandWeaponLocation != null)
                GetOffHandWeaponLocation.gameObject.SetActive(false);

            bool WeaponIsDrawn = CURRENT_HOLSTER_STATE == HOLSTER_STATE.DRAWN;
            Debug.Log($"CURRENT_HOLSTER_STATE: {CURRENT_HOLSTER_STATE}, WeaponIsDrawn: {WeaponIsDrawn}");

            if (_hasMainHandWeapon)
            {
                WeaponItem Mainhand = GetMainHandWeapon();
                if (Mainhand != null)
                {
                    int mainHolsterIndex = GetHolsterIndex(Mainhand, true);
                    if (HolsterLocations != null)
                    {
                        if (mainHolsterIndex < 0 || mainHolsterIndex >= HolsterLocations.Count)
                        {
                            Debug.LogError($"Main hand holster index out of range: {mainHolsterIndex}");
                        }
                        else
                        {
                            HolsterLocations[mainHolsterIndex].gameObject.SetActive(!WeaponIsDrawn);
                            Debug.Log($"Main hand holster location {mainHolsterIndex} active: {!WeaponIsDrawn}");
                        }
                    }
                    // Safely activate all children of main hand weapon location
                    if (GetMainHandWeaponLocation != null)
                    {
                        for (int i = 0; i < GetMainHandWeaponLocation.childCount; i++)
                        {
                            var child = GetMainHandWeaponLocation.GetChild(i).gameObject;
                            child.SetActive(WeaponIsDrawn);
                            Debug.Log($"Main hand weapon child {i} active: {WeaponIsDrawn}");
                        }
                    }
                }
            }

            if (_hasOffHandWeapon)
            {
                EquipmentItem Offhand = GetOffHandWeapon();
                if (Offhand != null)
                {
                    int offHolsterIndex = GetHolsterIndex(Offhand, false);
                    if (HolsterLocations != null)
                    {
                        if (offHolsterIndex < 0 || offHolsterIndex >= HolsterLocations.Count)
                        {
                            Debug.LogError($"Off hand holster index out of range: {offHolsterIndex}");
                        }
                        else
                        {
                            HolsterLocations[offHolsterIndex].gameObject.SetActive(!WeaponIsDrawn);
                            Debug.Log($"Off hand holster location {offHolsterIndex} active: {!WeaponIsDrawn}");
                        }
                    }
                    // Safely activate all children of off hand weapon location
                    if (GetOffHandWeaponLocation != null)
                    {
                        for (int i = 0; i < GetOffHandWeaponLocation.childCount; i++)
                        {
                            var child = GetOffHandWeaponLocation.GetChild(i).gameObject;
                            child.SetActive(WeaponIsDrawn);
                            Debug.Log($"Off hand weapon child {i} active: {WeaponIsDrawn}");
                        }
                    }
                }
            }

            IsSwappingWeapon = false;
        }

        public DrawHolsterAnimationTriggers EvaluateHolsterAnimation()
        {
            WeaponItem mainWeapon = GetMainHandWeapon();
            WeaponItem offWeapon = GetOffHandWeapon();

            DrawHolsterAnimationTriggers mainTrigger = mainWeapon != null
                ? mainWeapon.GetEquipmentTrigger(true)
                : DrawHolsterAnimationTriggers.NONE;

            DrawHolsterAnimationTriggers offTrigger = offWeapon != null
                ? offWeapon.GetEquipmentTrigger(false)
                : DrawHolsterAnimationTriggers.NONE;

            if (_hasMainHandWeapon && !_hasOffHandWeapon)
                return mainTrigger;

            if (_hasMainHandWeapon && _hasOffHandWeapon && mainWeapon != null && offWeapon != null)
            {
                switch (mainWeapon.hands)
                {
                    case ScriptableItem.HandsRequired.ONE_HANDED:
                        switch (offWeapon.hands)
                        {
                            case ScriptableItem.HandsRequired.ONE_HANDED: return DrawHolsterAnimationTriggers.MixHilts;
                            case ScriptableItem.HandsRequired.TWO_HANDED: return DrawHolsterAnimationTriggers.MixHiltBack;
                            default: return DrawHolsterAnimationTriggers.MainHand1;
                        }
                    case ScriptableItem.HandsRequired.TWO_HANDED:
                        switch (offWeapon.hands)
                        {
                            case ScriptableItem.HandsRequired.ONE_HANDED: return DrawHolsterAnimationTriggers.MixBackHilt;
                            case ScriptableItem.HandsRequired.TWO_HANDED: return DrawHolsterAnimationTriggers.MixBackBack;
                            default: return DrawHolsterAnimationTriggers.MainHand2;
                        }
                    default:
                        return mainWeapon.GetEquipmentTrigger(true);
                }
            }

            return offTrigger;
        }

        // Toggle weapon holster state forcibly
        [ClientCallback]
        public void ForceHolsterState(HOLSTER_STATE state)
        {
            if (!isLocalPlayer) return;
            if (CURRENT_HOLSTER_STATE == HOLSTER_STATE.SHEATHED && !CanDrawWeapon()) return;
            CmdUpdateHolster(state);
        }

        // Returns true if player can draw weapon (not dead, allowed, not swapping)
        public bool CanDrawWeapon()
        {
            return player.state != "DEAD" && AllowedWeaponDrawState && !IsSwappingWeapon;
        }

        [ClientCallback]
        public void TryChangeHolsterState()
        {
            if (!isLocalPlayer) return;
            if (CURRENT_HOLSTER_STATE == HOLSTER_STATE.SHEATHED && !CanDrawWeapon()) return;

            bool isDrawn = CURRENT_HOLSTER_STATE == HOLSTER_STATE.DRAWN;
            CmdUpdateHolster(isDrawn ? HOLSTER_STATE.SHEATHED : HOLSTER_STATE.DRAWN);
        }

        [ServerCallback]
        public void ForceHolsterFromServer(HOLSTER_STATE state)
        {
            CURRENT_HOLSTER_STATE = state;
        }

        [Command]
        public void CmdUpdateHolster(HOLSTER_STATE state)
        {
            CURRENT_HOLSTER_STATE = state;
        }

        public int GetHolsterIndex(EquipmentItem data, bool isMainHand)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            switch (data.weaponType)
            {
                case ScriptableItem.WeaponType.Staff:
                case ScriptableItem.WeaponType.Bow:
                    return 4; // Back slot for 2H weapons
                case ScriptableItem.WeaponType.Shield:
                    return 5; // Shield slot
                default:
                    return isMainHand
                        ? (data.hands == ScriptableItem.HandsRequired.ONE_HANDED ? 1 : 0)
                        : (data.hands == ScriptableItem.HandsRequired.ONE_HANDED ? 3 : 2);
            }
        }

        // ---- Additional combat mode and animation logic ----

        public void CloseAll()
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("closeLeft", true);
                animator.SetBool("closeRight", true);
            }

            onehandmainhandsheath?.SetActive(false);
            onehandoffhandsheath?.SetActive(false);
            twohandmainhandsheath?.SetActive(false);
            twohandoffhandsheath?.SetActive(false);
            staffsheath?.SetActive(false);
            shieldsheath?.SetActive(false);
        }

        public bool combatMode = true;
        public string mainHandAni;
        public string offHandAni;

        [Server]
        public void ServerToggleCombat(bool combatActive, bool playAni)
        {
            RpcToggleCombat(combatActive, playAni);
        }

        [Command]
        public void CmdToggleCombat(bool combatActive, bool playAni)
        {
            RpcToggleCombat(combatActive, playAni);
        }

        [ClientRpc]
        public void RpcToggleCombat(bool combatActive, bool playAni)
        {
            ToggleCombat(combatActive, playAni);
        }

        public void ToggleCombat(bool combatActive, bool playAni)
        {
            combatMode = combatActive;

            bool hasRightWeapon = player.equipment.slots[22].amount != 0;
            bool hasLeftWeapon = player.equipment.slots[23].amount != 0;

            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("closeRight", combatActive && hasRightWeapon);
                animator.SetBool("closeLeft", combatActive && hasLeftWeapon);

                if (!combatActive)
                {
                    animator.SetBool("closeRight", false);
                    animator.SetBool("closeLeft", false);
                }
            }

            if (playAni)
                PlayAnimation();

            StartCoroutine(DelayedToggle(combatActive));
        }

        public void PlayAnimation()
        {
            mainHandAni = "";
            offHandAni = "";

            if (player.equipment.slots[22].amount != 0)
            {
                var hands = GetHands();
                switch (hands)
                {
                    case ScriptableItem.HandsRequired.ONE_HANDED:
                        mainHandAni = "1MainHand";
                        break;
                    case ScriptableItem.HandsRequired.TWO_HANDED:
                        mainHandAni = "2MainHand";
                        break;
                    default:
                        mainHandAni = "1MainHand";
                        break;
                }
            }

            if (player.equipment.slots[23].amount != 0)
            {
                var offhandItem = player.equipment.slots[23].item.data;
                if (offhandItem != null)
                {
                    var offhandType = offhandItem.weaponType;
                    var hands = GetHands();

                    if (hands == ScriptableItem.HandsRequired.ONE_HANDED)
                    {
                        offHandAni = offhandType == ScriptableItem.WeaponType.Shield ? "Shield" : "1OffHand";
                    }
                    else if (hands == ScriptableItem.HandsRequired.TWO_HANDED)
                    {
                        offHandAni = "2OffHand";
                    }
                }
            }
        



        Debug.Log(mainHandAni + ":main    " + offHandAni + ":offhand      before changing into mix");

            if (mainHandAni != "" && offHandAni != "")
            {
                if (mainHandAni == "1MainHand" && offHandAni == "1OffHand") { mainHandAni = "MixHilts"; offHandAni = ""; }//1
                else if (mainHandAni == "2MainHand" && offHandAni == "2OffHand") { mainHandAni = "MixBackBack"; offHandAni = ""; }

                else if (mainHandAni == "1MainHand" && offHandAni == "2OffHand") { mainHandAni = "MixHiltBack"; offHandAni = ""; }//2
                else if (mainHandAni == "2MainHand" && offHandAni == "1Offhand") { mainHandAni = "MixBackHilt"; offHandAni = ""; }//3

                else if (mainHandAni == "1MainHand" && offHandAni == "Shield") { mainHandAni = "MixHiltShield"; offHandAni = ""; }
                else if (mainHandAni == "2MainHand" && offHandAni == "Shield") { mainHandAni = "MixBackShield"; offHandAni = ""; }

                if (mainHandAni == "1MainHandGun" && offHandAni == "1OffHandGun") { mainHandAni = "MixHilts"; offHandAni = ""; }//1
                else if (mainHandAni == "2MainHandGun" && offHandAni == "2OffHandGun") { mainHandAni = "MixBackBack"; offHandAni = ""; }

                else if (mainHandAni == "1MainHandGun" && offHandAni == "2OffHandGun") { mainHandAni = "MixHiltBack"; offHandAni = ""; }//2
                else if (mainHandAni == "2MainHandGun" && offHandAni == "1OffHandGun") { mainHandAni = "MixBackHilt"; offHandAni = ""; }//3

                else if (mainHandAni == "1MainHandGun" && offHandAni == "Shield") { mainHandAni = "MixHiltShield"; offHandAni = ""; }
                else if (mainHandAni == "2MainHandGun" && offHandAni == "Shield") { mainHandAni = "MixBackShield"; offHandAni = ""; }

                offHandAni = "";
            }
            Debug.Log(mainHandAni);
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                if (mainHandAni != "")
                {
                    animator.SetTrigger(mainHandAni);
                }
                if (offHandAni != "")
                {
                    animator.SetTrigger(offHandAni);//use triggers
                }
            }//this might be the way lmao
        }
        IEnumerator<WaitForSeconds> DelayedToggle(bool combatActive)
        {
            yield return new WaitForSeconds(0.6f);//give time for animation to play ^^
            combatMode = combatActive;
            if (combatMode)
            {
                mainhand.SetActive(true);
                offhand.SetActive(true);



                CloseAll(); //just close all 
            }
            else
            {
                mainhand.SetActive(false);
                offhand.SetActive(false);
                OpenAll(); //always close all, so we can only set true what we do have   //its checking slots before theyreefreshlocation



                //mainhand
                // MAINHAND
                if (player.equipment.slots[22].amount != 0)
                {
                    WeaponItem mainWeapon = player.equipment.slots[22].item.data as WeaponItem;
                    if (mainWeapon != null)
                    {
                        if (mainWeapon.hands == ScriptableItem.HandsRequired.ONE_HANDED)
                        {
                            switch (mainWeapon.weaponType)
                            {

                                default:
                                    onehandmainhandsheath.SetActive(true); // Generic 1h weapon sheath
                                    break;
                            }
                        }
                        else if (mainWeapon.hands == ScriptableItem.HandsRequired.TWO_HANDED)
                        {
                            switch (mainWeapon.weaponType)
                            {
                                case ScriptableItem.WeaponType.Staff:
                                    staffsheath.SetActive(true); // Special sheath for staff
                                    break;

                                default:
                                    twohandmainhandsheath.SetActive(true); // Generic 2h weapon sheath
                                    break;
                            }
                        }
                    }
                }

                // OFFHAND
                if (player.equipment.slots[23].amount != 0)
                {
                    WeaponItem offWeapon = player.equipment.slots[23].item.data as WeaponItem;
                    if (offWeapon != null)
                    {
                        Debug.Log($"[Sheath] Offhand weapon: {offWeapon.name}, Type: {offWeapon.weaponType}, Hands: {offWeapon.hands}");

                        if (offWeapon.hands == ScriptableItem.HandsRequired.ONE_HANDED)
                        {
                            switch (offWeapon.weaponType)
                            {
                                case ScriptableItem.WeaponType.Shield:
                                    Debug.Log("[Sheath] Activating shield sheath.");

                                    shieldsheath?.SetActive(true);

                                    break;

                                default:
                                    Debug.Log("[Sheath] Activating one-handed offhand sheath.");
                                    onehandoffhandsheath?.SetActive(true);
                                    break;
                            }
                        }
                        else if (offWeapon.hands == ScriptableItem.HandsRequired.TWO_HANDED)
                        {
                            switch (offWeapon.weaponType)
                            {
                                case ScriptableItem.WeaponType.Staff:
                                    Debug.Log("[Sheath] Activating staff sheath.");
                                    staffsheath?.SetActive(true);
                                    break;

                                default:
                                    Debug.Log("[Sheath] Activating two-handed offhand sheath.");
                                    twohandoffhandsheath?.SetActive(true);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Offhand weapon is null or not a WeaponItem.");
                    }
                }

            }
        }






        public ScriptableItem.HandsRequired GetHands()
        {
            int index = GetEquippedWeaponIndex();
            return pe.slots[index].item.data.hands;
        }

        /// <summary>
        /// refresh location logic
        /// </summary>
        /// <returns></returns>
        /// 

        //UMA SHIT###################################################################################


        //########################################################################################################################################################




        //###################################################################################################################################################################


        public struct DnaChunkMessage : NetworkMessage
        {
            public int chunkIndex;
            public int totalChunks;
            public string chunkData;
        }

        // Store received chunks indexed by player/netId or just current reception
        private List<string> receivedChunks = new List<string>();

        // Public callback to invoke when full DNA string is reassembled
        public Action<string> OnFullDnaReceived;

        // Maximum chunk size per message to avoid message too large error
        private const int chunkSize = 60000;

        #region Compression Helpers

        public static string CompressString(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        public static string DecompressString(string compressedBase64)
        {
            byte[] compressed = Convert.FromBase64String(compressedBase64);
            using (var msi = new MemoryStream(compressed))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        #endregion

        #region Chunk Helpers

        private static string[] SplitStringIntoChunks(string str, int maxChunkSize)
        {
            int totalChunks = (str.Length + maxChunkSize - 1) / maxChunkSize;
            string[] chunks = new string[totalChunks];
            for (int i = 0; i < totalChunks; i++)
            {
                int start = i * maxChunkSize;
                int length = Math.Min(maxChunkSize, str.Length - start);
                chunks[i] = str.Substring(start, length);
            }
            return chunks;
        }

        private static string JoinChunks(string[] chunks)
        {
            return string.Concat(chunks);
        }

        #endregion

        // Call this to send DNA string from client to server
        public void SendDnaString(string dnaString)
        {
            if (!isLocalPlayer) return;
            StartCoroutine(SendDnaRoutine(dnaString));
        }

        private IEnumerator SendDnaRoutine(string dnaString)
        {
            string compressed = CompressString(dnaString);
            string[] chunks = SplitStringIntoChunks(compressed, chunkSize);

            for (int i = 0; i < chunks.Length; i++)
            {
                CmdSendDnaChunk(i, chunks.Length, chunks[i]);
                yield return null; // spread messages across frames to avoid flooding
            }
        }

        // Command to send a chunk from client to server
        [Command]
        private void CmdSendDnaChunk(int chunkIndex, int totalChunks, string chunkData)
        {
            // Optionally: Save on server, relay to others, etc.
            // For now: relay back to client that sent it to confirm reception
            TargetReceiveDnaChunk(connectionToClient, chunkIndex, totalChunks, chunkData);
        }

        // TargetRpc to send chunk from server to client (you can also make this ClientRpc if broadcasting)
        [TargetRpc]
        private void TargetReceiveDnaChunk(NetworkConnection target, int chunkIndex, int totalChunks, string chunkData)
        {
            if (receivedChunks.Count == 0)
                receivedChunks = new List<string>(new string[totalChunks]);

            receivedChunks[chunkIndex] = chunkData;

            // Check if all chunks received
            if (receivedChunks.FindIndex(s => s == null) == -1)
            {
                string fullCompressed = JoinChunks(receivedChunks.ToArray());
                string fullDna = DecompressString(fullCompressed);

                // Clear for next reception
                receivedChunks.Clear();

                // Fire callback for whoever needs the full DNA string now
                OnFullDnaReceived?.Invoke(fullDna);
            }
        }

        public void RefreshLocation(int index)
        {
            isDrawn = true;
            ItemSlot slot = pe.slots[index];
            EquipmentInfo info = pe.slotInfo[index];
            if (slot.amount > 0)
            {
                ScriptableItem baseItem = slot.item.dataCostume ?? slot.item.data;

                if (baseItem is EquipmentItem equipmentItem)
                {
                    if (equipmentItem.maleUmaRecipe != null)
                        dca.SetSlot(equipmentItem.maleUmaRecipe);
                    else if (equipmentItem.femaleUmaRecipe != null)
                        dca.SetSlot(equipmentItem.femaleUmaRecipe);
                }
                else if (baseItem is WeaponItem weaponItem)
                {
                    if (weaponItem.maleUmaRecipe != null)
                        dca.SetSlot(weaponItem.maleUmaRecipe);
                    else if (weaponItem.femaleUmaRecipe != null)
                        dca.SetSlot(weaponItem.femaleUmaRecipe);
                }

                if (index == (int)EquipmentSlotLocations.MainHand || index == (int)EquipmentSlotLocations.OffHand)
                {
                    WeaponItem itemData = (WeaponItem)baseItem; // safer cast

                    // Treat weapon as drawn when equipping to avoid spawning holster prefab
                    // Force drawn on equip

                    if (!isDrawn && itemData.modelPrefab != null)
                    {
                        int holster = GetHolsterIndex(itemData, index == (int)EquipmentSlotLocations.MainHand);

                        foreach (Transform child in HolsterLocations[holster])
                        {
                            if (child.name == "CURRENT_HOLSTER")
                                Destroy(child.gameObject);
                        }

                        GameObject holsterObject = Instantiate(itemData.modelPrefab, HolsterLocations[holster], false);
                        holsterObject.name = "CURRENT_HOLSTER";
                    }
                    else
                    {
                        int holster = GetHolsterIndex(itemData, index == (int)EquipmentSlotLocations.MainHand);
                        foreach (Transform child in HolsterLocations[holster])
                        {
                            if (child.name == "CURRENT_HOLSTER")
                                Destroy(child.gameObject);
                        }
                        GameObject holsterObject = Instantiate(itemData.modelPrefab, HolsterLocations[holster], false);
                        holsterObject.name = "CURRENT_HOLSTER";

                    }
                }
            }
        }





        public character_appearance GetDefaultAppearance(Player player)
        {
            return new character_appearance
            {
                character = player.name,

                hairIndex = 0,
                beardIndex = 0,
                eyebrowIndex = 0,
                tattooIndex = 0,

                hairColor = "black",
                eyeColor = "blue",
                skinColor = "lightbrown"
            };
        }







        public Color ParseHtmlColorSafe(string html)
        {
            if (ColorUtility.TryParseHtmlString("#" + html, out var color))
                return color;
            return Color.white;
        }


        [Command(requiresAuthority = false)]
        public void CmdSaveUMAAppearanceToServer(string compressedDna, int hair, int beard, int eyebrow, int tattoo,
             string hairColorHex, string eyeColorHex, string skinColorHex)
        {
            Debug.Log("CmdSaveUMAAppearanceToServer called.");

            var player = GetComponent<Player>();
            var bs = player.GetComponent<battlescripts>();
            if (bs == null)
            {
                Debug.LogWarning("Battlescripts component missing.");
                return;
            }

            currentDnaRecipe = compressedDna;
            hairIndex = hair;
            beardIndex = beard;
            eyebrowIndex = eyebrow;
            tattooIndex = tattoo;

            // Load existing appearance to prevent wiping saved colors
            var existingApp = Database.singleton.LoadCharacterAppearance(player.name);
            if (existingApp == null)
            {
                existingApp = new character_appearance();
                existingApp.character = player.name;
            }

            // Only update colors if they are provided (non-empty)
            existingApp.hairIndex = hair;
            existingApp.beardIndex = beard;
            existingApp.eyebrowIndex = eyebrow;
            existingApp.tattooIndex = tattoo;

            if (!string.IsNullOrEmpty(hairColorHex))
                existingApp.hairColor = hairColorHex;

            if (!string.IsNullOrEmpty(eyeColorHex))
                existingApp.eyeColor = eyeColorHex;

            if (!string.IsNullOrEmpty(skinColorHex))
                existingApp.skinColor = skinColorHex;

            Debug.Log($"Saving appearance to DB: hair={existingApp.hairIndex}, beard={existingApp.beardIndex}, eyebrow={existingApp.eyebrowIndex}, tattoo={existingApp.tattooIndex}, eyes=#{existingApp.eyeColor}, skin=#{existingApp.skinColor}, hair=#{existingApp.hairColor}");

            // Save the updated appearance
            Database.singleton.SaveCharacterAppearance(existingApp);

            // Update battlescripts fields as well
            bs.hairColor = existingApp.hairColor;
            bs.eyeColor = existingApp.eyeColor;
            bs.skinColor = existingApp.skinColor;
        }




        [Command(requiresAuthority = false)]
        public void CmdSetStyleIndexes(int hair, int beard, int eyebrow, int tattoo)
        {
            Debug.Log($"[SERVER] Set style indexes: Hair={hair}, Beard={beard}, Eyebrow={eyebrow}, Tattoo={tattoo}");
            hairIndex = hair;
            beardIndex = beard;
            eyebrowIndex = eyebrow;
            tattooIndex = tattoo;
        }





        public void LoadPreviewAppearances(character_appearance app)
        {
            Debug.Log("LoadPreviewAppearances");

            hairIndex = app.hairIndex;
            beardIndex = app.beardIndex;
            eyebrowIndex = app.eyebrowIndex;
            tattooIndex = app.tattooIndex;

            Debug.Log($"Loading indices for {name}: HairIndex={hairIndex}, BeardIndex={beardIndex}, EyebrowIndex={eyebrowIndex}, TattooIndex={tattooIndex}");

            if (dca != null)
            {
                dca.ClearSlots();

                var barber = UI_BarberShop.Instance;
                if (barber == null)
                {
                    Debug.LogError("Barber instance is null!");
                    return;
                }

                if (hairIndex >= 0 && hairIndex < barber.maleHairStyles.Count)
                    dca.SetSlot(barber.maleHairStyles[hairIndex]);

                if (beardIndex >= 0 && beardIndex < barber.maleBeardStyles.Count)
                    dca.SetSlot(barber.maleBeardStyles[beardIndex]);

                if (eyebrowIndex >= 0 && eyebrowIndex < barber.maleEyebrowStyles.Count)
                    dca.SetSlot(barber.maleEyebrowStyles[eyebrowIndex]);

                if (tattooIndex >= 0 && tattooIndex < barber.maleTattooStyles.Count)
                    dca.SetSlot(barber.maleTattooStyles[tattooIndex]);

                // Apply colors
                if (!string.IsNullOrEmpty(app.hairColor) &&
                    ColorUtility.TryParseHtmlString(app.hairColor.StartsWith("#") ? app.hairColor : "#" + app.hairColor, out Color parsedHair))
                {
                    dca.SetColor("Hair", parsedHair);
                }

                if (!string.IsNullOrEmpty(app.skinColor) &&
                    ColorUtility.TryParseHtmlString(app.skinColor.StartsWith("#") ? app.skinColor : "#" + app.skinColor, out Color parsedSkin))
                {
                    dca.SetColor("Skin", parsedSkin);
                }

                if (!string.IsNullOrEmpty(app.eyeColor) &&
                    ColorUtility.TryParseHtmlString(app.eyeColor.StartsWith("#") ? app.eyeColor : "#" + app.eyeColor, out Color parsedEye))
                {
                    dca.SetColor("Eyes", parsedEye);
                }

                Debug.Log($"Colors applied: Hair={app.hairColor}, Skin={app.skinColor}, Eyes={app.eyeColor}");

                // Final build
                dca.UpdateColors(true);
                dca.BuildCharacter(true);

                Debug.Log($"Finished LoadPreviewAppearances: hairIndex={hairIndex}, beardIndex={beardIndex}, etc.");
            }

            RefreshUMAEquipmentVisuals();
        }


        public void RefreshUMAEquipmentVisuals()
        {
            Debug.Log("refresumaequipmentvisuals");

            string[] equipmentSlots = new string[]
{
    "Chest", "Legs", "Shoulders", "Gloves", "Feet", "Helmet", "Chest", "Tabard", "Shirt", "Tie", "Waist"
};

            foreach (string slotName in equipmentSlots)
            {
                dca.ClearSlot(slotName);
            }

            // Apply saved appearance slots first (hair, beard, eyebrow, tattoo)


            // Then add equipment slots as usual
            EquipmentInfo[] slotInfo = ((PlayerEquipment)player.equipment).slotInfo;
            ItemSlot[] currentSlots = player.equipment.slots.ToArray();

            for (int i = 0; i < currentSlots.Length; i++)
            {
                ItemSlot slot = currentSlots[i];
                if (slot.amount > 0)
                {
                    ScriptableItem baseItem = slot.item.dataCostume ?? slot.item.data;

                    if (baseItem is EquipmentItem equipmentItem)
                    {
                        for (int j = 0; j < slotInfo.Length; j++)
                        {
                            if (slotInfo[j].requiredCategory == equipmentItem.category)
                            {
                                if (equipmentItem.maleUmaRecipe != null)
                                    dca.SetSlot(equipmentItem.maleUmaRecipe);
                                else if (equipmentItem.femaleUmaRecipe != null)
                                    dca.SetSlot(equipmentItem.femaleUmaRecipe);
                                break;
                            }
                        }
                    }
                    else if (baseItem is WeaponItem weaponItem)
                    {
                        if (weaponItem.maleUmaRecipe != null)
                            dca.SetSlot(weaponItem.maleUmaRecipe);
                        else if (weaponItem.femaleUmaRecipe != null)
                            dca.SetSlot(weaponItem.femaleUmaRecipe);
                    }
                }
            }

            // Parse string hex colors to Unity Colors here

            //dca.UpdateColors(true);
            dca.BuildCharacter(true);
        }
        [Command(requiresAuthority = false)]
        public void CmdRefreshEquipmentSlots()
        {
            slots = player.equipment.slots.ToArray();
            player.equipment.slots.Clear();
            player.equipment.slots.AddRange(slots);
        }

        public void RequestAppearance()
        {
            var app = Database.singleton.LoadCharacterAppearance(player.name);
            if (app == null)
            {
                Debug.Log("default data used");
                app = Database.GetDefaultAppearance();
            }

            Debug.Log($"RequestAppearance for {name}: HairIndex={app.hairIndex}, BeardIndex={app.beardIndex}, EyebrowIndex={app.eyebrowIndex}, " +
                      $"HairColor=#{app.hairColor}, EyeColor=#{app.eyeColor}, SkinColor=#{app.skinColor}");

            // Remove connectionToClient here – just call locally
            ApplyAppearance(app);
        }
        //[TargetRpc]
        public void ApplyAppearance(character_appearance app)
        {
            StartCoroutine(DelayedApply(app));
        }





        [Command]
        public void CmdRequestAppearance()
        {
            var app = Database.singleton.LoadCharacterAppearance(name);
            if (app == null)
            {
                Debug.Log("default data used");
                app = Database.GetDefaultAppearance();
            }

            Debug.Log($"[SERVER] CmdRequestAppearance for {name}: HairIndex={app.hairIndex}, BeardIndex={app.beardIndex}, EyebrowIndex={app.eyebrowIndex}, " +
                      $"HairColor=#{app.hairColor}, EyeColor=#{app.eyeColor}, SkinColor=#{app.skinColor}");

            TargetReceiveAppearance(connectionToClient,
                app.hairIndex, app.beardIndex, app.eyebrowIndex, app.tattooIndex,
                app.hairColor, app.eyeColor, app.skinColor);
        }

        [TargetRpc]
        public void TargetReceiveAppearance(NetworkConnection target,
            int hairIndex, int beardIndex, int eyebrowIndex, int tattooIndex,
            string hairColor, string eyeColor, string skinColor)
        {
            // Remove database loads here - they should only happen on the server.
            character_appearance app = new character_appearance
            {
                hairIndex = hairIndex,
                beardIndex = beardIndex,
                eyebrowIndex = eyebrowIndex,
                tattooIndex = tattooIndex,
                hairColor = hairColor,
                eyeColor = eyeColor,
                skinColor = skinColor
            };

            StartCoroutine(DelayedApply(app));
        }

        private IEnumerator DelayedApply(character_appearance app)
        {
            yield return new WaitForSeconds(0.1f); // optional buffer time
            LoadPreviewAppearances(app);
        }

       




        //onstartclient stuff
        //###################################################################################


        //########################################################################################################################################################




        //###################################################################################################################################################################


        private IEnumerator ActivatePreviewerWithDelay(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);

            MyEquipmentPreviewer previewer = GameObject.FindObjectOfType<MyEquipmentPreviewer>(true);
            if (previewer != null)
            {
                previewer.gameObject.SetActive(true);
                Debug.Log("EquipmentViewer GameObject activated after delay");
            }
            else
            {
                Debug.LogError("EquipmentPreviewer component not found.");
            }
        }
        public override void OnStartClient()
        {

            Debug.Log("onstartclient called");
            //StartCoroutine(ApplyAppearanceWhenReady());
            // setup synclist callbacks on client. no need to update and show and
            // animate equipment on server
#pragma warning disable CS0618
            pe.slots.Callback += equipmentchanger;
#pragma warning restore CS0618

            // refresh all locations once (on synclist changed won't be called
            // for initial lists)
            // -> needs to happen before ProximityChecker's initial SetVis call,
            //    otherwise we get a hidden character with visible equipment
            //    (hence OnStartClient and not Start)
            for (int i = 0; i < pe.slots.Count; ++i)
                RefreshLocation(i);
            CmdRefreshEquipmentSlots();
            //RefreshUMAPreviewEquipmentVisuals();


            barber = FindFirstObjectByType<UI_BarberShop>();
            if (barber == null)
            {
                Debug.LogError("UI_BarberShop component not found in the scene!");
            }


            Player player = Player.localPlayer;
            if (player == null)
                return;

            dca = player.GetComponentInChildren<DynamicCharacterAvatar>();
            if (dca != null)
            {
                Debug.Log("apply savedappearances");
                // SyncVars are already loaded by Mirror by this point,
                // so just apply saved appearance directly
                //ApplySavedAppearance();
            }

            CloseAll();
            HandleCapeAnimation();

            if (_hasMainHandWeapon || _hasOffHandWeapon)
            {
                animator.SetBool("closeRight", _hasMainHandWeapon);
                animator.SetBool("closeLeft", _hasOffHandWeapon);
            }

            if (animator != null)
            {
                if (CURRENT_HOLSTER_STATE == HOLSTER_STATE.DRAWN)
                    OnHolsterChanged(HOLSTER_STATE.SHEATHED, HOLSTER_STATE.DRAWN);
                else
                    OnHolsterChanged(HOLSTER_STATE.DRAWN, HOLSTER_STATE.SHEATHED);
            }


        }

        //PARTY PARTY?###################################################################################


        //########################################################################################################################################################




        //###################################################################################################################################################################


        //CAPE modifier


        public void HandleCapeAnimation(Player player = null)
        {
            if (player == null)
            {
                if (!NetworkClient.active || Player.localPlayer == null)
                    return;

                player = Player.localPlayer;
            }

            if (capeObject == null)
            {
                Debug.LogError("capeObject is null.");
                return;
            }

            if (wingObject == null)
            {
                Debug.LogError("wingObject is null.");
                return;
            }

            // Cape equipment check
            if (player.equipment.slots.Count > 6 && player.equipment.slots[6].amount > 0)
            {
                Animator capeAnimator = capeObject.GetComponentInChildren<Animator>();
                if (capeAnimator != null)
                {
                    RuntimeAnimatorController capeController = Resources.Load<RuntimeAnimatorController>("Controllers/fattestcapecontroller");
                    if (capeController != null)
                    {
                        capeAnimator.runtimeAnimatorController = capeController;
                        capeAnimator.Rebind();
                        capeAnimator.Update(0f);
                    }
                }
            }

            // Wing equipment check
            if (player.equipment.slots.Count > 7 && player.equipment.slots[7].amount > 0 && jet != null)
            {
                switch (jet.currentEquipment)
                {
                    case JetpacksNWings.EquipmentType.Jetpack:
                        jet.equipmentObject.SetActive(false); // Disable in preview
                        WearingWings = true;
                        break;

                    case JetpacksNWings.EquipmentType.AngelWings:
                        RebindWeaponnWingsAnimator(wingObject, "Controllers/capewingcontroller");
                        WearingWings = true;
                        break;

                    case JetpacksNWings.EquipmentType.DemonWings:
                        RebindWeaponnWingsAnimator(wingObject, "Controllers/DEMONcapewingcontroller");
                        WearingWings = true;
                        break;

                    default:
                        jet.equipmentObject.SetActive(false);
                        WearingWings = false;
                        break;
                }
            }
            else
            {
                WearingWings = false;
            }

            // Chainsaw
            if (player.equipment.slots.Count > 22 && player.equipment.slots[22].amount > 0)
            {
                if (weaponObject != null)
                {
                    RebindWeaponnWingsAnimator(weaponObject, "Controllers/orcchainsaw");
                }
            }
        }

        /// <summary>
        /// Rebinds the animator for a specific object without affecting the player's main animator.
        /// </summary>
        private void RebindWeaponnWingsAnimator(GameObject targetObject, string animatorPath)
        {
            Animator targetAnimator = targetObject.GetComponentInChildren<Animator>();
            if (targetAnimator != null)
            {
                RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(animatorPath);
                if (controller != null)
                {
                    targetAnimator.runtimeAnimatorController = controller;
                }
                else
                {
                    Debug.LogError($"Animator controller not found: {animatorPath}");
                }
            }

        }

        void RebindAnimator(string controllerPath)
        {

            Animator wingAnimator = jet.equipmentObject.GetComponent<Animator>();

            if (wingAnimator != null)
            {
                RuntimeAnimatorController wingController = Resources.Load<RuntimeAnimatorController>(controllerPath);
                if (wingController != null)
                {
                    wingAnimator.runtimeAnimatorController = wingController;
                    //Debug.Log("Wing Animator rebind successful");
                }
                else
                {
                    //Debug.LogError("Wing Controller not found at path: " + controllerPath);
                }
            }
            else
            {
                Debug.LogError("Wing Animator not found.");
            }
        }
    }


   




    public partial class WeaponItem : EquipmentItem
    {
        private DrawHolsterAnimationTriggers StandardLogic(bool MainHand)
        {
            if (MainHand)
            {
                return hands == HandsRequired.ONE_HANDED ? DrawHolsterAnimationTriggers.MainHand1 : DrawHolsterAnimationTriggers.MainHand2;
            }
            else
            {
                return hands == HandsRequired.ONE_HANDED ? DrawHolsterAnimationTriggers.OffHand1 : DrawHolsterAnimationTriggers.OffHand2;
            }
        }

        public DrawHolsterAnimationTriggers GetEquipmentTrigger(bool MainHand)
        {
            switch (weaponType)
            {
                case WeaponType.Sword:
                    return StandardLogic(MainHand);

                case WeaponType.Axe:
                    return StandardLogic(MainHand);

                case WeaponType.Mace:
                    return StandardLogic(MainHand);

                case WeaponType.Fist:
                    return StandardLogic(MainHand);

                case WeaponType.Spear:
                    return StandardLogic(MainHand);

                case WeaponType.Dagger:
                    return StandardLogic(MainHand);

                case WeaponType.Shield:
                    return StandardLogic(!MainHand);

                case WeaponType.Staff:
                    return StandardLogic(!MainHand); // assuming there are 1h staffs?  there are wands

                case WeaponType.Bow:
                    return StandardLogic(!MainHand);

                case WeaponType.Wand:
                    return StandardLogic(MainHand);

                case WeaponType.Gun:
                    return StandardLogic(MainHand);

                case WeaponType.Tome:
                    return StandardLogic(MainHand);

                case WeaponType.None:
                    return DrawHolsterAnimationTriggers.NONE;

                default: return DrawHolsterAnimationTriggers.NONE;
            }
        }
    }

    public enum DrawHolsterAnimationTriggers : int
    {
        Unarmed,
        MainHand1,
        MainHand2,
        OffHand1,
        OffHand2,
        Shield,
        Staff,
        MixHilts,
        MixBackBack,
        MixHiltBack,
        MixBackHilt,
        NONE
    }

    // tooltip



    public partial class PlayerCharacterControllerMovement
    {
        public battlescripts bs;


        //special movements
        [Header("Crouching")]
        public float crouchSpeed = 1.5f;
        public float crouchAcceleration = 5; // set to maxint for instant speed
        public float crouchDeceleration = 10; // feels best if higher than acceleration
        public KeyCode crouchKey = KeyCode.V;
        public bool crouchKeyPressed;

        [Header("Sneaking")]
        public float sneakSpeed = 1.5f;
        public float sneakAcceleration = 5; // set to maxint for instant speed
        public float sneakDeceleration = 10; // feels best if higher than acceleration
        public KeyCode sneakKey = KeyCode.I;
        public bool sneakKeyPressed;


        [Header("Crawling")]
        public float crawlSpeed = 1;
        public float crawlAcceleration = 5; // set to maxint for instant speed
        public float crawlDeceleration = 10; // feels best if higher than acceleration
        public KeyCode crawlKey = KeyCode.B;
        public bool crawlKeyPressed;

        [Header("Walking")]
        public float walkSpeed = 1;
        public float walkAcceleration = 5; // set to maxint for instant speed
        public float walkDeceleration = 10; // feels best if higher than acceleration
        public KeyCode walkKey = KeyCode.K;
        public bool walkKeyPressed;

        [Header("Climbing")]
        public float climbSpeed = 3;
        Collider ladderCollider;
        Collider wallCollider;
        private ScriptableSkill _data;
        [Header("Airborne")]
        float flyingSpeed = 120;
        [Header("AngelMount")]
        float angelflyingSpeed = 150;
        public PlayerMountControl mountcontrol;
        public bool isAngel = false;


        public bool IsANGELMounted()
        {
            return mountcontrol.activeMount != null && mountcontrol.activeMount.health.current > 0 && isAngel;
        }

        public bool EventANGELMounted()
        {
            return IsANGELMounted();
        }

        bool EventCrouchToggle()
        {
            return crouchKeyPressed;
        }
        bool EventSneakToggle()
        {
            return sneakKeyPressed;
        }

        bool EventCrawlToggle()
        {
            return crawlKeyPressed;
        }

        bool EventWalkToggle()
        {
            return walkKeyPressed;
        }

        bool EventLadderEnter()
        {
            return ladderCollider != null;
        }

        bool EventLadderExit()
        {
            // OnTriggerExit isn't good enough to detect ladder exits because we
            // shouldn't exit as soon as our head sticks out of the ladder collider.
            // only if we fully left it. so check this manually here:
            return ladderCollider != null &&
                   (!ladderCollider.bounds.Intersects(controllerCollider.bounds) ||
                   (Input.GetAxis("Vertical") < 0 && controller.isGrounded));
        }

        bool EventWallEnter()
        {
            return wallCollider != null;
        }

        bool EventWallExit()
        {
            // OnTriggerExit isn't good enough to detect ladder exits because we
            // shouldn't exit as soon as our head sticks out of the ladder collider.
            // only if we fully left it. so check this manually here:
            return wallCollider != null &&
                   !wallCollider.bounds.Intersects(controllerCollider.bounds);
        }

        /// <summary>
        /// original movestate manipulation
        /// </summary>
        /// <param name="inputDir"></param>
        /// <param name="desiredDir"></param>
        /// <returns></returns>
        /// 





        public MoveState AdjustSwimming(Vector2 inputDir, Vector3 desiredDir)
        {
            Debug.Log("adjust swimming");

            // Check if the left mouse button is being held
            bool isLMBHeld = Input.GetMouseButton(0);

            Vector3 direction = Vector3.zero;

            if (!isLMBHeld)
            {
                // Get movement direction from camera (ignoring vertical component)
                direction = Camera.main.transform.forward * inputDir.y +
                            Camera.main.transform.right * inputDir.x;
                direction.y = 0; // Prevent unintended vertical movement
            }
            else
            {
                // When LMB is held, use the desired direction (based on player input, no camera influence)
                direction = desiredDir;
            }

            // Apply horizontal movement
            moveDir.x = direction.x * swimSpeed;
            moveDir.z = direction.z * swimSpeed;

            if (inWater)
            {
                // Handle vertical movement (Jump & Crouch)
                if (waterCollider != null)
                {
                    float surface = waterCollider.bounds.max.y;
                    float surfaceDirection = surface - controller.bounds.min.y - swimSurfaceOffset;
                    float surfaceDistance = ((surface - swimSurfaceOffset) - transform.position.y);

                    // If the player is close to the surface and not jumping, move downward
                    if (surfaceDistance < 0.25f && !Input.GetButton("Jump") && !EventFalling())
                    {
                        moveDir.y = -2f;
                        return MoveState.SWIMMING; // Return the state here
                    }

                    bool onSurface = surfaceDistance < 0.3f;

                    // Handle breath system (only switch if on or off surface)
                    if (!onSurface)
                        GetComponent<Breath>().Switch(true);
                    else
                        GetComponent<Breath>().Switch(false);

                    // If no input is active
                    if (!UIUtils.AnyInputActive())
                    {
                        if (inputDir.x != 0 || inputDir.y != 0)
                        {
                            // Get direction from camera
                            direction = Camera.main.transform.forward * inputDir.y +
                                        Camera.main.transform.right * inputDir.x;

                            moveDir.y = direction.y * swimSpeed;

                            // If on the surface and not pressing jump or crouch, stay at surface
                            if (onSurface && direction.y > 0 && !Input.GetButton("Jump") && !Input.GetKey(KeyCode.LeftControl))
                                moveDir.y = 0f;
                        }
                        else
                        {
                            // If no input, make the player float lightly when off surface
                            moveDir.y = onSurface ? 0f : swimSpeed * 0.2f;
                        }

                        // Handle jump and dive movement
                        if (Input.GetButton("Jump"))
                            moveDir.y += swimSpeed;
                        else if (Input.GetKey(KeyCode.LeftControl))
                            moveDir.y += -swimSpeed;
                    }
                    else
                    {
                        moveDir.y = onSurface ? 0f : swimSpeed * 0.2f;
                    }
                }
                else
                {
                    moveDir.y = 0; // If no water collider, reset vertical movement
                }
            }

            return MoveState.SWIMMING; // Return the default state after swimming logic
        }

        public MoveState AdjustMountedSwimming(Vector2 inputDir, Vector3 desiredDir)
        {
            // Check if the left mouse button is being held
            bool isLMBHeld = Input.GetMouseButton(0);

            Vector3 direction = Vector3.zero;

            if (!isLMBHeld)
            {
                // Get movement direction from camera (ignoring vertical component)
                direction = Camera.main.transform.forward * inputDir.y +
                            Camera.main.transform.right * inputDir.x;
                direction.y = 0; // Prevent unintended vertical movement
            }
            else
            {
                // When LMB is held, use the desired direction (based on player input, no camera influence)
                direction = desiredDir;
            }

            // Apply horizontal movement
            moveDir.x = direction.x * swimSpeed;
            moveDir.z = direction.z * swimSpeed;

            if (inWater)
            {
                // Handle vertical movement (Jump & Crouch)
                if (waterCollider != null)
                {
                    float surface = waterCollider.bounds.max.y;
                    float surfaceDirection = surface - controller.bounds.min.y - swimSurfaceOffset;
                    float surfaceDistance = ((surface - swimSurfaceOffset) - transform.position.y);

                    // If the player is close to the surface and not jumping, move downward
                    if (surfaceDistance < 0.25f && !Input.GetButton("Jump") && !EventFalling())
                    {
                        moveDir.y = -2f;
                        return MoveState.MOUNTED_SWIMMING; // Return the state here
                    }

                    bool onSurface = surfaceDistance < 0.3f;

                    // Handle breath system (only switch if on or off surface)
                    if (!onSurface)
                        GetComponent<Breath>().Switch(true);
                    else
                        GetComponent<Breath>().Switch(false);

                    // If no input is active
                    if (!UIUtils.AnyInputActive())
                    {
                        if (inputDir.x != 0 || inputDir.y != 0)
                        {
                            // Get direction from camera
                            direction = Camera.main.transform.forward * inputDir.y +
                                        Camera.main.transform.right * inputDir.x;

                            moveDir.y = direction.y * swimSpeed;

                            // If on the surface and not pressing jump or crouch, stay at surface
                            if (onSurface && direction.y > 0 && !Input.GetButton("Jump") && !Input.GetKey(KeyCode.LeftControl))
                                moveDir.y = 0f;
                        }
                        else
                        {
                            // If no input, make the player float lightly when off surface
                            moveDir.y = onSurface ? 0f : swimSpeed * 0.2f;
                        }

                        // Handle jump and dive movement
                        if (Input.GetButton("Jump"))
                            moveDir.y += swimSpeed;
                        else if (Input.GetKey(KeyCode.LeftControl))
                            moveDir.y += -swimSpeed;
                    }
                    else
                    {
                        moveDir.y = onSurface ? 0f : swimSpeed * 0.2f;
                    }
                }
                else
                {
                    moveDir.y = 0; // If no water collider, reset vertical movement
                }
            }

            return MoveState.MOUNTED_SWIMMING; // Return the default state after swimming logic
        }





        public MoveState UpdateMOUNTEDextended(Vector2 inputDir, Vector3 desiredDir)
        {

            bs.CmdToggleCombat(false, false);
            UpdateMOUNTED(inputDir, desiredDir);
            // find mounted speed if mount is still around
            // (it might not be immediately after dismounting, in which case
            //  UpdateMOUNTED gets called one more time until the EventDismounted()
            //  check below)
            float speed = mountControl.activeMount != null
                          ? mountControl.activeMount.speed
                          : runSpeed;

            // move
            moveDir.x = desiredDir.x * speed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * speed;

            if (EventDied())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                // DEAD in any case, even if rescale failed
                return MoveState.DEAD;
            }
            else if (EventFalling())
            {
                return MoveState.MOUNTED_AIRBORNE;
            }
            else if (EventJumpRequested())
            {
                // start the jump movement into Y dir, go to jumping
                // note: no endurance>0 check because it feels odd if we can't jump
                moveDir.y = jumpSpeed;
                PlayJumpSound();
                return MoveState.MOUNTED_AIRBORNE;
            }
            else if (EventDismounted())
            {
                bs.CmdToggleCombat(true, false);
                return MoveState.IDLE;

            }
            else if (EventUnderWater())
            {
                return MoveState.MOUNTED_SWIMMING;
            }
            else if (EventMounted()) { } // don't care

            return MoveState.MOUNTED;
        }

        public MoveState UpdateMOUNTED_AIRBORNEextended(Vector2 inputDir, Vector3 desiredDir)
        {

            //UpdateMOUNTED_AIRBORNE(inputDir, desiredDir);
            // recalculate desired direction while ignoring inputDir horizontal part
            // (horses can't strafe.)
            //desiredDir = GetDesiredDirection(new Vector2(0, inputDir.y));

            // horizontal input axis rotates the character instead of strafing


            // find mounted speed if mount is still around
            // (it might not be immediately after dismounting, in which case
            //  UpdateMOUNTED gets called one more time until the EventDismounted()
            //  check below)
            /*float speed = mountControl.activeMount != null
                ? mountControl.activeMount.speed
                : runSpeed;

            // move
            moveDir.x = desiredDir.x * speed;
            // bMMORPGAddon : SwimflyBasic
            if (mountControl.activeMount != null && !mountControl.activeMount.flyingMount)
                moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * speed;
            */
            // bMMORPGAddon : SwimflyBasic
            if (mountControl.activeMount != null && mountControl.activeMount.flyingMount)
            {
                // move
                moveDir.x = desiredDir.x * flyingSpeed;
                moveDir.y = 0;
                moveDir.z = desiredDir.z * flyingSpeed;

                if (!UIUtils.AnyInputActive())
                {
                    if (inputDir.x != 0 || inputDir.y != 0)
                    {
                        Vector3 direction = camera.transform.forward * inputDir.y +
                                            camera.transform.right * inputDir.x;
                        moveDir.y = direction.y * swimSpeed;
                    }

                    if (Input.GetButton("Jump"))
                        moveDir.y += flyingSpeed;
                    else if (Input.GetKey(KeyCode.LeftControl))
                        moveDir.y += -flyingSpeed;
                }
            }


            return MoveState.MOUNTED_AIRBORNE;
        }
        public MoveState UpdateAIRBORNEextended(Vector2 inputDir, Vector3 desiredDir)
        {

            //UpdateAIRBORNE(inputDir, desiredDir);
            // input allowed while airborne?

            if (bs.WearingWings)
            {

                airborneSteering = false;
                //Debug.Log("wearing wings");
                moveDir.x = desiredDir.x * flyingSpeed;
                moveDir.y = 0;
                moveDir.z = desiredDir.z * flyingSpeed;

                if (!UIUtils.AnyInputActive())
                {
                    if (inputDir.x != 0 || inputDir.y != 0)
                    {
                        Vector3 direction = camera.transform.forward * inputDir.y +
                                            camera.transform.right * inputDir.x;
                        moveDir.y = direction.y * swimSpeed;
                    }

                    if (Input.GetButton("Jump"))
                        moveDir.y += flyingSpeed;
                    else if (Input.GetKey(KeyCode.LeftControl))
                        moveDir.y += -flyingSpeed;
                }

            }

            return MoveState.AIRBORNE;
        }
        public MoveState UpdateIDLEextended(Vector2 inputDir, Vector3 desiredDir)
        {

            UpdateIDLE(inputDir, desiredDir);

            // move
            // (moveDir.xz can be set to 0 to have an interruption when landing)

            if (EventWalkToggle())
            {
                // rescale capsule

                return MoveState.WALKING;
            }

            else if (EventCrouchToggle())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false);
                return MoveState.CROUCHING;
            }
            else if (EventSneakToggle())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false);
                return MoveState.SNEAKING;
            }
            else if (EventCrawlToggle())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.CRAWLING;
            }
            else if (EventLadderEnter())
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
            else if (EventMounted())
            {
                return MoveState.MOUNTED;
            }
            else if (EventANGELMounted())
            {
                return MoveState.ANGELMOUNTED;
            }

            else if (EventUnderWater())
            {
                // rescale capsule
                if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
                {
                    return MoveState.SWIMMING;
                }
            }
            else if (inputDir != Vector2.zero)
            {
                return MoveState.RUNNING;
            }
            else if (EventDismounted()) { } // don't care

            return MoveState.IDLE;
        }

        public MoveState UpdateRUNNINGextended(Vector2 inputDir, Vector3 desiredDir)
        {
            UpdateRUNNING(inputDir, desiredDir);
            // QE key rotation


            // move

            if (EventWalkToggle())
            {
                // rescale capsule

                return MoveState.WALKING;
            }

            else if (EventCrouchToggle())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false);
                return MoveState.CROUCHING;
            }
            else if (EventSneakToggle())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false);
                return MoveState.SNEAKING;
            }
            else if (EventCrawlToggle())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.CRAWLING;
            }
            else if (EventLadderEnter())
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
            else if (EventMounted())
            {
                return MoveState.MOUNTED;
            }
            else if (EventANGELMounted())
            {
                return MoveState.ANGELMOUNTED;
            }

            else if (EventUnderWater())
            {
                // rescale capsule
                if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
                {
                    return MoveState.SWIMMING;
                }
            }
            // go to idle after fully decelerating (y doesn't matter)
            else if (moveDir.x == 0 && moveDir.z == 0)
            {
                return MoveState.IDLE;
            }
            else if (EventDismounted()) { } // don't care

            ProgressStepCycle(inputDir, runSpeed);
            return MoveState.RUNNING;
        }




        /// <summary>
        /// modified movestates
        /// </summary>
        /// <param name="inputDir"></param>
        /// <param name="desiredDir"></param>
        /// <returns></returns>


        void EnterLadder()
        {
            // make player look directly at ladder forward. but we also initialize
            // freelook manually already to overwrite the initial rotation, so
            // that in the end, the camera keeps looking at the same angle even
            // though we did modify transform.forward.
            // note: even though we set the rotation perfectly here, there's
            //       still one frame where it seems to interpolate between the
            //       new and the old rotation, which causes 1 odd camera frame.
            //       this could be avoided by overwriting transform.forward once
            //       more in LateUpdate.
            if (isLocalPlayer)
            {
                InitializeFreeLook();
                transform.forward = ladderCollider.transform.forward;
            }
        }

        public MoveState UpdateWALKING(Vector2 inputDir, Vector3 desiredDir)
        {
            RotateWithKeys();

            // move
            moveDir.x = desiredDir.x * walkSpeed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * walkSpeed;

            if (EventDied())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.DEAD;
            }
            else if (EventFalling())
            {
                return MoveState.AIRBORNE;
            }
            else if (EventJumpRequested())
            {
                // stop crawling when pressing jump key. this feels better than
                // jumping from the crawling state.

                // rescale capsule if possible
                if (controller.CanSetHeight(controller.defaultHeight * 1f, true))
                {
                    controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false);
                    return MoveState.AIRBORNE;
                }
            }
            else if (EventMounted())
            {
                return MoveState.MOUNTED;
            }
            else if (EventANGELMounted())
            {
                return MoveState.ANGELMOUNTED;
            }

            else if (EventCrouchToggle())
            {
                // rescale capsule if possible
                if (controller.CanSetHeight(controller.defaultHeight * 0.5f, true))
                {
                    // limit speed to crouch speed so we don't decelerate from run speed
                    // to crouch speed (hence crouching too fast for a short time)
                    // -> not allowing any speed > crouchspeed also makes speedhack
                    //    protection easier later on.
                    // rescale capsule
                    controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false);
                    return MoveState.CROUCHING;
                }
            }
            else if (EventSneakToggle())
            {
                // rescale capsule if possible
                return MoveState.SNEAKING;
            }
            else if (EventCrawlToggle())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.CRAWLING;
            }



            else if (EventUnderWater())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.SWIMMING;
            }

            else if (EventLadderEnter())
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
            else if (EventDismounted()) { } // don't care

            ProgressStepCycle(inputDir, crawlSpeed);
            return MoveState.WALKING;
        }

        public MoveState UpdateSNEAKING(Vector2 inputDir, Vector3 desiredDir)
        {
            // QE key rotation
            if (player.IsMovementAllowed())
                RotateWithKeys();
            // move
            moveDir.x = desiredDir.x * sneakSpeed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * sneakSpeed;


            if (EventDied())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.DEAD;
            }
            else if (EventFalling())
            {
                return MoveState.AIRBORNE;
            }
            else if (EventJumpRequested())
            {
                // stop crouching when pressing jump key. this feels better than
                // jumping from the crouching state.

                // rescale capsule if possible
                if (controller.CanSetHeight(controller.defaultHeight * 1f, true))
                {
                    controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false);
                    return MoveState.IDLE;
                }
            }
            else if (EventMounted())
            {
                return MoveState.MOUNTED;
            }
            else if (EventANGELMounted())
            {
                return MoveState.ANGELMOUNTED;
            }

            else if (EventCrouchToggle())
            {
                // rescale capsule if possible
                if (controller.CanSetHeight(controller.defaultHeight * 1f, true))
                {
                    controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false);
                    return MoveState.CROUCHING;
                }
            }
            else if (EventCrawlToggle())
            {
                // limit speed to crawl speed so we don't decelerate from run speed
                // to crawl speed (hence crawling too fast for a short time)
                // -> not allowing any speed > crawlspeed also makes speedhack
                //    protection easier later on.
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.CRAWLING;
            }

            else if (EventUnderWater())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.SWIMMING;
            }

            else if (EventLadderEnter())
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
            else if (EventDismounted()) { } // don't care

            ProgressStepCycle(inputDir, sneakSpeed);
            return MoveState.SNEAKING;
        }
        public MoveState UpdateCROUCHING(Vector2 inputDir, Vector3 desiredDir)
        {
            // QE key rotation
            if (player.IsMovementAllowed())
                RotateWithKeys();
            // move
            moveDir.x = desiredDir.x * crouchSpeed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * crouchSpeed;


            if (EventDied())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.DEAD;
            }
            else if (EventFalling())
            {
                return MoveState.AIRBORNE;
            }
            else if (EventJumpRequested())
            {
                // stop crouching when pressing jump key. this feels better than
                // jumping from the crouching state.

                // rescale capsule if possible
                if (controller.CanSetHeight(controller.defaultHeight * 1f, true))
                {
                    controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false);
                    return MoveState.IDLE;
                }
            }
            else if (EventMounted())
            {
                return MoveState.MOUNTED;
            }
            else if (EventANGELMounted())
            {
                return MoveState.ANGELMOUNTED;
            }


            else if (EventSneakToggle())
            {

                return MoveState.SNEAKING;

            }
            else if (EventCrawlToggle())
            {
                // limit speed to crawl speed so we don't decelerate from run speed
                // to crawl speed (hence crawling too fast for a short time)
                // -> not allowing any speed > crawlspeed also makes speedhack
                //    protection easier later on.
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.CRAWLING;
            }

            else if (EventUnderWater())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.SWIMMING;
            }

            else if (EventLadderEnter())
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
            else if (EventDismounted()) { } // don't care

            ProgressStepCycle(inputDir, crouchSpeed);
            return MoveState.CROUCHING;
        }

        public MoveState UpdateCRAWLING(Vector2 inputDir, Vector3 desiredDir)
        {
            // QE key rotation
            if (player.IsMovementAllowed())
                RotateWithKeys();
            // move
            moveDir.x = desiredDir.x * crawlSpeed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * crawlSpeed;

            if (EventDied())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.DEAD;
            }
            else if (EventFalling())
            {
                return MoveState.AIRBORNE;
            }
            else if (EventJumpRequested())
            {
                // stop crawling when pressing jump key. this feels better than
                // jumping from the crawling state.

                // rescale capsule if possible
                if (controller.CanSetHeight(controller.defaultHeight * 1f, true))
                {
                    controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false);
                    return MoveState.IDLE;
                }
            }
            else if (EventMounted())
            {
                return MoveState.MOUNTED;
            }
            else if (EventANGELMounted())
            {
                return MoveState.ANGELMOUNTED;
            }

            else if (EventCrouchToggle())
            {
                // rescale capsule if possible
                if (controller.CanSetHeight(controller.defaultHeight * 0.5f, true))
                {
                    // limit speed to crouch speed so we don't decelerate from run speed
                    // to crouch speed (hence crouching too fast for a short time)
                    // -> not allowing any speed > crouchspeed also makes speedhack
                    //    protection easier later on.
                    // rescale capsule
                    controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false);
                    return MoveState.CROUCHING;
                }
            }
            else if (EventSneakToggle())
            {

                return MoveState.SNEAKING;

            }


            else if (EventUnderWater())
            {
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.SWIMMING;
            }

            else if (EventLadderEnter())
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
            else if (EventDismounted()) { } // don't care

            ProgressStepCycle(inputDir, crawlSpeed);
            return MoveState.CRAWLING;
        }

        public MoveState UpdateCLIMBING(Vector2 inputDir, Vector3 desiredDir)
        {
            if (EventDied())
            {
                // player rotation was adjusted to ladder rotation before.
                // let's reset it, but also keep look forward
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                ladderCollider = null;
                // rescale capsule
                controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
                return MoveState.DEAD;
            }
            // finished climbing?
            else if (EventLadderExit())
            {
                // player rotation was adjusted to ladder rotation before.
                // let's reset it, but also keep look forward
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                ladderCollider = null;
                return MoveState.IDLE;
            }
            else if (EventJumpRequested())
            {
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                ladderCollider = null;
                // rescale capsule if possible
                if (controller.CanSetHeight(controller.defaultHeight * 1f, true))
                {
                    controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false);
                    return MoveState.AIRBORNE;
                }
                return MoveState.AIRBORNE;
            }
            else if (EventMounted())
            {
                return MoveState.MOUNTED;
            }
            else if (EventANGELMounted())
            {
                return MoveState.ANGELMOUNTED;
            }

            else if (EventDismounted()) { } // don't care
                                            // interpret forward/backward movement as upward/downward
                                            // note: NO ACCELERATION, otherwise we would climb really fast when
                                            //       sprinting towards a ladder. and the actual climb feels way too
                                            //       unresponsive when accelerating.
            moveDir.y = inputDir.y * climbSpeed;
            moveDir.x = inputDir.x * climbSpeed;
            moveDir.z = 0;
            // make the direction relative to ladder rotation. so when pressing right
            // we always climb to the right of the ladder, no matter how it's rotated
            //moveDir = ladderCollider.transform.rotation * moveDir;
            moveDir = ladderCollider.transform.rotation * moveDir;
            //transform.rotation = ladderCollider.transform.rotation;
            Debug.DrawLine(transform.position, transform.position + moveDir, Color.yellow, 0.1f, false);


            return MoveState.CLIMBING;
        }






    }





















    public partial class PlayerSkills
    {
        private bool isCastingSkill = false;
        public Vector3 targetScale = new Vector3(2, 2, 2);
        private Vector3 initialScale = new Vector3(1, 1, 1);
        public TargetProjectileSkill rapidfire;
        private float timeElapsed = 0f;
        public float distance;
        public float duration = 2f;
        [Header("skillz")]
        public aoedamageskillstart aoestart;
        public GameObject jumplungeprefab;
        public GameObject whirlwindprefab;
        public GameObject frontflipprefab;
        public GameObject swingforwardprefab;
        public GameObject firegridprefab;
        public GameObject overheadprefab;
        public GameObject onemilpunchesprefab;
        public GameObject swing2prefab;
        //buff prefabs
        public GameObject critbusterprefab;
        public GameObject critvengprefab;
        public GameObject berserkprefab;
        public Entity caster;
        public int skillLevel;
        public float dashSpeed = 10f; // Adjust the speed to make it slower
        public float dashDuration = 0.5f; // Duration of the dash
        public float jumplungeDuration = 0.5f; // Duration of the dash
        public float gravity = -9.81f;
        public Animator animator;
        public Player player;
        //public GameObject trail;
        public CharacterController2k controller;
        private Vector3 velocity;
        private Skills skill;
        private bool skillExecuted = false;
        public battlescripts bs;
        public PlayerCharacterControllerMovement pccm;
        public float berserkScaleMultiplier = 2f;
        private bool isBerserkActive = false; // Track if Berserk is active
        void Awake()
        {
            //trail.SetActive(false);
        }



        public void Update()
        {
            int skillIndex = player.skills.currentSkill;

            if (player.state == "CASTING" && !isCastingSkill) // Check flag
            {
                if (skillIndex >= 0 && skillIndex < player.skills.skills.Count)
                {
                    Skill skill = player.skills.skills[skillIndex]; // Get the current skill
                    StartCastextended(skill);
                    isCastingSkill = true; // Set the flag
                }
            }
            else if (player.state != "CASTING") // Reset flag when not casting
            {

                if (isCastingSkill)
                {
                    // Stop animations for all skills
                    animator.SetBool("weapontoss", false);
                    animator.SetBool("rapidfire", false);
                    animator.SetBool("swing2", false);
                    animator.SetBool("dashforward", false);
                    animator.SetBool("Invisibility", false);
                    animator.SetBool("aoedamageskill", false);
                    animator.SetBool("MeteorShower", false);
                    animator.SetBool("swingforward", false);
                    animator.SetBool("1millionpunches", false);
                    animator.SetBool("frontflip", false);
                    animator.SetBool("jumplunge", false);
                    animator.SetBool("overhead", false);
                    animator.SetBool("criticalvengeance", false);
                    animator.SetBool("critbuster", false);
                    animator.SetBool("Berserk", false);

                    isCastingSkill = false; // Reset the casting flag
                    //trail.SetActive(false);
                }
            }
        }





        void StartCastextended(Skill skill)
        {


            if (skill.name == "rapidfire")
            {
                Debug.Log("rapidfire");
                startrapidfire();
            }


            if (skill.name == "swing2")
            {
                applyswing2();

            }
            if (skill.name == "dashforward")
            {
                applydashforward();
            }


            if (skill.name == "Invisibility")
            {
                applyinvisibility();
            }


            if (skill.name == "aoedamageskill")
            {

                Debug.Log("aoedamageskill called");
                startaoe();
                Instantiate(whirlwindprefab, player.transform.position + Vector3.up * 1.5f, Quaternion.identity, player.transform);
            }
            if (skill.name == "MeteorShower")
            {
                Instantiate(firegridprefab, player.transform.position + Vector3.up * 0f, Quaternion.identity, player.transform);

            }
            if (skill.name == "swingforward")
            {
                applyswingforward();
                StartCoroutine(swingforward());
            }

            if (skill.name == "1millionpunches")
            {
                startonemil();
                StartCoroutine(onemillionpunch());
            }

            if (skill.name == "frontflip")
            {
                StartCoroutine(frontflipSpawn());
            }

            if (skill.name == "jumplunge")
            {
                applyjumplunge();
                StartCoroutine(jumplungeSpawn());

            }

            if (skill.name == "overhead")
            {
                applyoverhead();
                StartCoroutine(overheadSpawn());

            }
            if (skill.name == "criticalvengeance")
            {
                applycritveng();
                StartCoroutine(critvengSpawn());

            }
            if (skill.name == "critbuster")
            {
                applycritbust();
                StartCoroutine(critbustSpawn());

            }
            //buffs
            if (skill.name == "Berserk")
            {
                Debug.Log("Berserk called");
                applyberserk(); // Apply any other Berserk effects

                // Start the coroutine for Berserk spawn and scaling transition
                StartCoroutine(BerserkSpawn());
            }
        }




        public Vector3 GetVelocity()
        {
            return velocity;
        }
        public void startswing2()
        {
            StartCoroutine(swing2());
        }
        public void endswing2()
        {
            StopCoroutine(swing2());
        }
        IEnumerator swing2()
        {
            yield return new WaitForSeconds(.5f);
            Debug.Log("attack 1: " + Time.time);
            aoestart.swingcast2(caster, skillLevel);
            yield return new WaitForSeconds(.5f);
            Debug.Log("attack 2: " + Time.time);
            aoestart.swingcast2(caster, skillLevel);
            yield return new WaitForSeconds(.5f);
            Debug.Log("attack 3: " + Time.time);
            aoestart.swingcast2(caster, skillLevel);
            yield return new WaitForSeconds(.75f);
            Debug.Log("attack 3: " + Time.time);
            aoestart.swingcast2(caster, skillLevel);
            endswing2();

        }
        public IEnumerator Berserk()
        {
            yield return new WaitForSeconds(15f);
            Debug.Log("attack 1: " + Time.time);
            //rotatingstuff.redOrb.SetActive(false);

            float scaleDownDuration = 1f;
            Debug.Log("Starting scale down.");
            yield return SmoothScale(player.transform, targetScale, initialScale, scaleDownDuration);
            Debug.Log("Scale down completed. Player scaled back to: " + player.transform.localScale);
            isBerserkActive = false;
            EndBerserk();

        }

        public void StartBerserk()
        {
            //rotatingstuff.redOrb.SetActive(true);
            StartCoroutine(Berserk());
        }

        public void EndBerserk()
        {
            StopAllCoroutines(); // This stops all coroutines, you might want to handle this more specifically
        }

        public void StartCritBust()
        {
            //rotatingstuff.purpleOrb.SetActive(true);
            StartCoroutine(CritBust());
        }

        public void EndCritBust()
        {
            StopAllCoroutines();
        }

        public IEnumerator CritBust()
        {
            yield return new WaitForSeconds(15f);
            Debug.Log("attack 1: " + Time.time);
            //rotatingstuff.purpleOrb.SetActive(false);
            EndCritBust();

        }

        public void StartCritVeng()
        {
            //rotatingstuff.blueOrb.SetActive(true);
            StartCoroutine(CritVeng());
        }

        public void EndCritVeng()
        {
            StopAllCoroutines();
        }

        public IEnumerator CritVeng()
        {
            yield return new WaitForSeconds(15f);
            Debug.Log("attack 1: " + Time.time);
            //rotatingstuff.blueOrb.SetActive(false);
            EndCritVeng();
        }
        public void startswing()
        {
            StartCoroutine(swingAttack());
        }
        public void endswing()
        {
            StopCoroutine(swingAttack());
        }
        IEnumerator swingAttack()
        {
            yield return new WaitForSeconds(.5f);
            Debug.Log("attack 1: " + Time.time);
            aoestart.swingcast(caster, skillLevel);
            yield return new WaitForSeconds(1f);
            Debug.Log("attack 2: " + Time.time);
            aoestart.swingcast(caster, skillLevel);
            yield return new WaitForSeconds(.5f);
            Debug.Log("attack 3: " + Time.time);
            aoestart.swingcast(caster, skillLevel);
            yield return new WaitForSeconds(.75f);
            Debug.Log("attack 3: " + Time.time);
            aoestart.swingcast(caster, skillLevel);
            endswing();

        }

        public void startoverhead()
        {
            StartCoroutine(overheadAttack());
        }
        public void endoverhead()
        {
            StopCoroutine(overheadAttack());
        }
        IEnumerator overheadAttack()
        {
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 1: " + Time.time);
            aoestart.overheadcast(caster, skillLevel);
            yield return new WaitForSeconds(.5f);
            Debug.Log("attack 2: " + Time.time);
            aoestart.overheadcast(caster, skillLevel);
            yield return new WaitForSeconds(1f);
            Debug.Log("attack 3: " + Time.time);
            aoestart.overheadcast(caster, skillLevel);
            endoverhead();

        }

        IEnumerator rapidfireAttack()
        {
            Debug.Log("attack 1: " + Time.time);
            rapidfire.Apply(caster, skillLevel);
            yield return new WaitForSeconds(.1f);
            Debug.Log("attack 2: " + Time.time);
            rapidfire.Apply(caster, skillLevel);
            yield return new WaitForSeconds(.1f);
            Debug.Log("attack 3: " + Time.time);
            rapidfire.Apply(caster, skillLevel);
            yield return new WaitForSeconds(.1f);
            Debug.Log("attack 4: " + Time.time);
            rapidfire.Apply(caster, skillLevel);
            yield return new WaitForSeconds(.1f);
            Debug.Log("attack 5: " + Time.time);
            rapidfire.Apply(caster, skillLevel);
            yield return new WaitForSeconds(.1f);
            Debug.Log("attack 6: " + Time.time);
            rapidfire.Apply(caster, skillLevel);
            yield return new WaitForSeconds(.1f);
            endrapidfire();
        }

        public void startrapidfire()
        {
            StartCoroutine(rapidfireAttack());

        }
        public void endrapidfire()
        {
            StopCoroutine(rapidfireAttack());
        }

        IEnumerator onemilAttack()
        {
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 1: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 2: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 3: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 4: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 5: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            yield return new WaitForSeconds(.25f);
            Debug.Log("attack 6: " + Time.time);
            aoestart.onemil(caster, skillLevel);
            endonemil();

        }

        public void startonemil()
        {
            StartCoroutine(onemilAttack());
        }
        public void endonemil()
        {
            StopCoroutine(onemilAttack());
        }
        public IEnumerator dashforward()
        {

            Debug.Log("attack 1: " + Time.time);
            yield return new WaitForSeconds(.5f);
            aoestart.dashforward(caster, skillLevel);
            enddashforward();
        }

        public void startdashforward()
        {
            StartCoroutine(dashforward());
        }

        public void enddashforward()
        {
            StopAllCoroutines();
        }
        public IEnumerator jumplunge()
        {

            Debug.Log("attack 1: " + Time.time);
            yield return new WaitForSeconds(1f);
            aoestart.jumplunge(caster, skillLevel);
            endjumplunge();
        }

        public void startjumplunge()
        {
            StartCoroutine(jumplunge());
        }

        public void endjumplunge()
        {
            StopAllCoroutines();
        }

        public IEnumerator aoeAttack()
        {

            Debug.Log("attack 1: " + Time.time);
            aoestart.cyclonecast(caster, skillLevel);
            yield return new WaitForSeconds(.2f);
            Debug.Log("attack 2: " + Time.time);
            aoestart.cyclonecast(caster, skillLevel);
            yield return new WaitForSeconds(.2f);
            Debug.Log("attack 3: " + Time.time);
            aoestart.cyclonecast(caster, skillLevel);

        }

        public void startaoe()
        {
            StartCoroutine(aoeAttack());
        }

        public void endaoe()
        {
            StopAllCoroutines();
        }



        IEnumerator swingforward()
        {
            yield return new WaitForSeconds(1.25f);
            Instantiate(swingforwardprefab, player.transform.position + Vector3.up * .5f, Quaternion.identity, player.transform);
        }
        IEnumerator onemillionpunch()
        {
            yield return new WaitForSeconds(.25f);
            Vector3 spawnPosition = player.transform.position + Vector3.up * .5f + player.transform.forward * 5f;
            GameObject instance = Instantiate(onemilpunchesprefab, spawnPosition, player.transform.rotation, player.transform);
            Destroy(instance, 3f);
        }

        IEnumerator frontflipSpawn()
        {
            yield return new WaitForSeconds(1.25f);
            Instantiate(frontflipprefab, transform.position + new Vector3(0, 1, 0), transform.rotation);
        }
        IEnumerator jumplungeSpawn()
        {
            yield return new WaitForSeconds(1.25f);
            Instantiate(jumplungeprefab, transform.position + new Vector3(0, 0, 0), transform.rotation);
        }
        IEnumerator overheadSpawn()
        {
            yield return new WaitForSeconds(1.75f);
            Vector3 spawnPosition = transform.position + transform.forward * 3f;

            // Instantiate the prefab at the calculated position
            Instantiate(overheadprefab, spawnPosition, transform.rotation);
        }





        IEnumerator BerserkSpawn()
        {
            // If Berserk is already active, exit to prevent conflicts
            if (isBerserkActive)
            {
                Debug.Log("Berserk is already active, exiting coroutine.");
                yield break;
            }

            isBerserkActive = true;
            Debug.Log("Berserk ability activated.");

            // Ensure player scale is reset before starting
            player.transform.localScale = initialScale;

            Vector3 targetScale = initialScale * berserkScaleMultiplier;

            // Gradually scale up over 1 second
            float scaleUpDuration = 1f;
            Debug.Log("Starting scale up.");
            yield return SmoothScale(player.transform, initialScale, targetScale, scaleUpDuration);
            Debug.Log("Scale up completed. Player scaled to: " + player.transform.localScale);

            // Wait for 15 seconds in the berserk state
            yield return new WaitForSeconds(15f);
            Debug.Log("Berserk ability duration ended.");

            // Gradually scale back down over 1 second
            yield return SmoothScale(player.transform, targetScale, initialScale, scaleUpDuration);
            Debug.Log("Scale down completed. Player returned to original size.");


        }

        IEnumerator SmoothScale(Transform target, Vector3 fromScale, Vector3 toScale, float duration)
        {
            float timeElapsed = 0f;
            while (timeElapsed < duration)
            {
                target.localScale = Vector3.Lerp(fromScale, toScale, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure the final scale is set
            target.localScale = toScale;
            Debug.Log("Final scale set to: " + target.localScale);
        }


        IEnumerator critvengSpawn()
        {
            yield return new WaitForSeconds(0f);
            Vector3 spawnPosition = player.transform.position;
            GameObject instance = Instantiate(critvengprefab, spawnPosition, player.transform.rotation, player.transform);
            Destroy(instance, 5f);
        }

        IEnumerator critbustSpawn()
        {
            yield return new WaitForSeconds(0f);
            Vector3 spawnPosition = player.transform.position;
            GameObject instance = Instantiate(critbusterprefab, spawnPosition, player.transform.rotation, player.transform);
            Destroy(instance, 5f);
        }

        IEnumerator critbusttimer()
        {
            yield return new WaitForSeconds(3f);
            //animator.applyRootMotion = false;
        }


        IEnumerator swingforwardtimer()
        {
            yield return new WaitForSeconds(4f);
            animator.applyRootMotion = false;
        }
        IEnumerator swing2timer()
        {
            yield return new WaitForSeconds(3f);
            animator.applyRootMotion = false;
        }

        IEnumerator critvengtimer()
        {
            yield return new WaitForSeconds(3f);
            //animator.applyRootMotion = false;
        }

        IEnumerator berserktimer()
        {
            yield return new WaitForSeconds(3f);
            //animator.applyRootMotion = false;
        }
        IEnumerator jumplungetimer()
        {

            yield return new WaitForSeconds(.5f);
            float elapsedTime = 0f;
            Vector3 dashDirection = transform.forward; // Capture the current forward direction

            Debug.Log("Dash Direction: " + dashDirection); // Log the dash direction to check if it's correct

            while (elapsedTime < jumplungeDuration)
            {
                // Calculate dash movement
                Vector3 dashMove = dashDirection * dashSpeed * Time.deltaTime;

                // Apply gravity
                if (!controller.isGrounded)
                {
                    velocity.y += gravity * Time.deltaTime;
                }

                // Move the character
                controller.Move(dashMove + velocity * Time.deltaTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            animator.applyRootMotion = false;
            //trail.SetActive(false);
        }


        IEnumerator overheadtimer()
        {
            yield return new WaitForSeconds(.5f);
            float elapsedTime = 0f;
            Vector3 dashDirection = transform.forward; // Capture the current forward direction

            Debug.Log("Dash Direction: " + dashDirection); // Log the dash direction to check if it's correct

            while (elapsedTime < jumplungeDuration)
            {
                // Calculate dash movement
                Vector3 dashMove = dashDirection * dashSpeed * Time.deltaTime;

                // Apply gravity
                if (!controller.isGrounded)
                {
                    velocity.y += gravity * Time.deltaTime;
                }

                // Move the character
                //controller.Move(dashMove + velocity * Time.deltaTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            //animator.applyRootMotion = false;
            //trail.SetActive(false);
        }

        IEnumerator dashforwardtimer()
        {
            float elapsedTime = 0f;
            Vector3 dashDirection = transform.forward; // Capture the current forward direction

            Debug.Log("Dash Direction: " + dashDirection); // Log the dash direction to check if it's correct

            while (elapsedTime < dashDuration)
            {
                // Calculate dash movement
                Vector3 dashMove = dashDirection * dashSpeed * Time.deltaTime;

                // Apply gravity
                if (!controller.isGrounded)
                {
                    velocity.y += gravity * Time.deltaTime;
                }

                // Move the character
                controller.Move(dashMove + velocity * Time.deltaTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            animator.applyRootMotion = false;
            //trail.SetActive(false);
        }


        IEnumerator invisibilitytimer()
        {
            yield return new WaitForSeconds(30f);
            //pccm.state = MoveState.IDLE;

        }










        public void applyswingforward()
        {
            startswing();
            animator.applyRootMotion = true;
            StartCoroutine(swingforwardtimer());

        }
        public void applyswing2()
        {
            startswing2();
            animator.applyRootMotion = true;
            StartCoroutine(swing2timer());

        }

        public void applycritbust()
        {
            StartCritBust();
            //animator.applyRootMotion = true;
            StartCoroutine(critbusttimer());

        }

        public void applycritveng()
        {
            StartCritVeng();
            //animator.applyRootMotion = true;
            StartCoroutine(critvengtimer());

        }

        public void applyberserk()
        {
            StartBerserk();
            //animator.applyRootMotion = true;
            StartCoroutine(berserktimer());

        }
        public void applyoverhead()
        {


            //trail.SetActive(true);




            //animator.applyRootMotion = true;
            startoverhead();
            StartCoroutine(overheadtimer());

        }
        public void applydashforward()
        {


            //trail.SetActive(true);




            animator.applyRootMotion = true;
            startdashforward();
            StartCoroutine(dashforwardtimer());

        }
        public void applyinvisibility()
        {

            //pccm.state = MoveState.SNEAKING;


            StartCoroutine(invisibilitytimer());

        }
        public void applyjumplunge()
        {


            //trail.SetActive(true);




            animator.applyRootMotion = true;
            startjumplunge();
            StartCoroutine(jumplungetimer());

        }
    }



    //MOUNTED COMBAT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


    public partial class Player
    {
        public PlayerNpcCostume npcCostume;

        [Server]
        string cmUpdateServer_IDLE()
        {

            if (EventSkillRequest())
            {
                // don't cast while mounted
                // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)

                Skill skill = skills.skills[skills.currentSkill];
                if (skills.CastCheckSelf(skill) &&
                    skills.CastCheckTarget(skill) &&
#if _iMMO2D
                    skills.CastCheckDistance(skill, out Vector2 destination))
#else
                    skills.CastCheckDistance(skill, out Vector3 destination))
#endif
                {
                    //Debug.Log("MOVING->EventSkillRequest: early cast started while sliding to destination...");
                    // movement.Reset(); <- DO NOT DO THIS.
                    skills.StartCast(skill);
                    return "CASTING";
                }

            }
            if (EventSkillFinished()) { } // don't care
            if (EventMoveEnd()) { } // don't care
            if (EventTradeDone()) { } // don't care
            if (EventCraftingDone()) { } // don't care
            if (EventRespawn()) { } // don't care
            if (EventTargetDied()) { } // don't care
            if (EventTargetDisappeared()) { } // don't care

            return "IDLE"; // nothing interesting happened
        }

        [Server]
        string cmUpdateServer_MOVING()
        {

            if (EventSkillRequest())
            {
                // don't cast while mounted
                // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)

                Skill skill = skills.skills[skills.currentSkill];
                if (skills.CastCheckSelf(skill) &&
                    skills.CastCheckTarget(skill) &&
#if _iMMO2D
                    skills.CastCheckDistance(skill, out Vector2 destination))
#else
                    skills.CastCheckDistance(skill, out Vector3 destination))
#endif
                {
                    //Debug.Log("MOVING->EventSkillRequest: early cast started while sliding to destination...");
                    // movement.Reset(); <- DO NOT DO THIS.
                    skills.StartCast(skill);
                    return "CASTING";
                }

            }
            if (EventMoveStart()) { } // don't care
            if (EventSkillFinished()) { } // don't care
            if (EventTradeDone()) { } // don't care
            if (EventCraftingDone()) { } // don't care
            if (EventRespawn()) { } // don't care
            if (EventTargetDied()) { } // don't care
            if (EventTargetDisappeared()) { } // don't care

            return "MOVING"; // nothing interesting happened
        }


    }


    public partial class Npc : Entity
    {
        // Field for IsArenaMaster that will show in the Inspector
        [SerializeField]

        public NpcCostume costume;
        // Enum for NPC Type
        public enum NpcType
        {
            Regular,
            DungeonMaster,
            ArenaMaster
        }

        // Field for NPC Type that will show in the Inspector
        [SerializeField]
        private NpcType type;

        // Accessor for IsArenaMaster


        // Accessor for Type
        public NpcType Type
        {
            get { return type; }
            set { type = value; }
        }

        // Custom method for NPC-specific interaction logic
        public void Interactextended()
        {
            Debug.Log("Interact method called.");  // Check if method is called

            Player player = Player.localPlayer;
            if (player == null)
            {
                Debug.LogError("Player reference is null.");
                return;
            }

            if (player.health == null)
            {
                Debug.LogError("Player health reference is null.");
                return;
            }

            Debug.Log($"Player health: {player.health.current}, ClosestDistance: {Utils.ClosestDistance(player, this)}");

            if (player.health.current > 0 &&
                Utils.ClosestDistance(player, this) <= player.interactionRange)
            {
                Debug.Log($"NPC Type: {Type}");  // Log NPC Type

                // Check the NPC type and interact accordingly
                switch (Type)
                {
                    case NpcType.ArenaMaster:
                        UIArenaNpcDialogue.singleton.Show();
                        break;
                    case NpcType.Regular:
                        Debug.Log("Interacting with a regular NPC.");
                        UINpcDialogue.singleton.Show();
                        break;
                    default:
                        Debug.LogWarning("Unexpected NPC type.");
                        break;
                }
            }
            else
            {
                Debug.Log("Player is not close enough or health is zero.");
            }
        }
        void OnMouseDown()
        {
            // joined world yet? (not character selection?)
            // not over UI? (avoid targeting through windows)
            // and in a state where local player can select things?
            if (Player.localPlayer != null &&
                !Utils.IsCursorOverUserInterface() &&
                (Player.localPlayer.state == "IDLE" ||
                 Player.localPlayer.state == "MOVING" ||
                 Player.localPlayer.state == "CASTING" ||
                 Player.localPlayer.state == "STUNNED"))
            {
                // clear requested skill in any case because if we clicked
                // somewhere else then we don't care about it anymore
                Player.localPlayer.useSkillWhenCloser = -1;

                // set indicator in any case
                // (not just the first TimeLogout, because we might have clicked on the
                //  ground in the mean TimeLogout. always set it when selecting.)
                Player.localPlayer.indicator.SetViaParent(transform);

                // clicked for the first TimeLogout: SELECT
                if (Player.localPlayer.target != this)
                {
                    // target it
                    Player.localPlayer.CmdSetTarget(netIdentity);

                    // call OnSelect + hook
                    OnSelect();
                    onSelect.Invoke();
                }
                // clicked for the second TimeLogout: INTERACT
                else
                {
                    // call OnInteract + hook
                    Interactextended();
                    onInteract.Invoke();
                }
            }
        }
    }


    public partial class Mount

    {
        public PlayerCharacterControllerMovement pccm;
        public bool flyingMount = false;
        private new void Start()
        {
            animator.SetBool("OnGround", true);
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetBool("OnGround", true);
            }

        }

    }

    public partial class PlayerEquipment
    {
        public GameObject cameraHome;
    }
    public partial class UIEquipment
    {
        private PlayerEquipment equipCam;
        private float currentY = 0;

        public void rotateEquipCamera(bool leftOrRight)
        {
            equipCam = Player.localPlayer.GetComponent<
        PlayerEquipment>();
            if (leftOrRight == true)
            {
                currentY += 45;
                equipCam.cameraHome.transform.rotation =
                Quaternion.Euler(0, currentY, 0);
            }
            else
            {
                currentY -= 45;
                equipCam.cameraHome.transform.rotation =
                Quaternion.Euler(0, currentY, 0);
            }
            //StartCoroutine(equipCam.renderEquip());

        }
    }

    public partial class UI_Skillbar : MonoBehaviour
    {



        public battlescripts bs;



#if _CLIENT
    public void LateUpdate()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            bs = player.GetComponent<battlescripts>();
            int nbrSlots = player.skillbar.slots.Length;
            panel.SetActive(true);

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, nbrSlots, content);

            // refresh all
            for (int i = 0; i < nbrSlots; ++i)
            {
                SkillbarEntry entry = player.skillbar.slots[i];

                UISkillbarSlot slot = content.GetChild(i).GetComponent<UISkillbarSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index

                // hotkey overlay (without 'Alpha' etc.)
                string pretty = entry.hotKey.ToString().Replace("Alpha", "");
                slot.hotkeyText.text = pretty;

                // skill, inventory item or equipment item?
                int skillIndex = player.skills.GetSkillIndexByName(entry.reference);
                int inventoryIndex = player.inventory.GetItemIndexByName(entry.reference);
                int equipmentIndex = player.equipment.GetItemIndexByName(entry.reference);

                /**
                 * Is a Skill
                 */
                if (skillIndex != -1)
                {
                    Skill skill = player.skills.skills[skillIndex];
                    bool canCast = player.skills.CastCheckSelf(skill);
                    bool isAOE = skill.isAOE;
                    bs.cursorSelect = bs.isTargeting;

                    // if movement does NOT support navigation then we need to
                    // check distance too. otherwise distance doesn't matter
                    // because we can navigate anywhere.
                    if (!player.movement.CanNavigate())
#if _iMMO2D
                        canCast &= player.skills.CastCheckDistance(skill, out Vector2 _);
#else
                        canCast &= player.skills.CastCheckDistance(skill, out Vector3 _);
#endif
                    // hotkey pressed and not typing in any input right now?
                    if (Input.GetKeyDown(entry.hotKey) &&
                        !UIUtils.AnyInputActive() &&
                        canCast && !isAOE) // checks mana, cooldowns, etc.) {
                    {
                        // try use the skill or walk closer if needed
                        ((PlayerSkills)player.skills).TryUse(skillIndex);
                        Debug.Log(" is not AOE");
                    }
                    if (Input.GetKeyDown(entry.hotKey) &&
                        !UIUtils.AnyInputActive() &&
                        canCast && isAOE)
                    {
                        activateCircleIndicator(player);
                        bs.storedSkillIndex = skillIndex;
                        Debug.Log(" is AOE" + "skill index is " + bs.storedSkillIndex);
                    }

                    if (bs.cursorSelect && bs.targetIndicator.enabled == true && isAOE && !UIUtils.AnyInputActive() &&
                      canCast && Input.GetMouseButtonDown(0))
                    {
                        clickTargeting(player);
                        ((PlayerSkills)player.skills).TryUse(bs.storedSkillIndex);
                        Debug.Log("click targeting activated.");
                    }

                    // refresh skill slot
                    slot.button.interactable = canCast; // check mana, cooldowns, etc.
                    slot.button.onClick.SetListener(() =>
                    {
                        // try use the skill or walk closer if needed
                        if (!isAOE)
                        {
                            ((PlayerSkills)player.skills).TryUse(skillIndex);
                        }
                        else if (canCast && !bs.cursorSelect)
                        {
                            activateCircleIndicator(player);
                            bs.storedSkillIndex = skillIndex;
                            Debug.Log("Skill icon clicked: is AOE, skill index is " + bs.storedSkillIndex);
                        }
                    });
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = skill.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = skill.image;
                    float cooldown = skill.CooldownRemaining();
                    slot.cooldownOverlay.SetActive(cooldown > 0);
                    slot.cooldownText.text = cooldown.ToString("F0");
                    slot.cooldownCircle.fillAmount = skill.cooldown > 0 ? cooldown / skill.cooldown : 0;
                    slot.amountOverlay.SetActive(false);
                }

                /**
                 * Is a item in Inventory
                 */
                else if (inventoryIndex != -1)
                {
                    ItemSlot itemSlot = player.inventory.slots[inventoryIndex];

                    // hotkey pressed and not typing in any input right now?
                    if (Input.GetKeyDown(entry.hotKey) && !UIUtils.AnyInputActive())
                        player.inventory.CmdUseItem(inventoryIndex);

                    // refresh inventory slot
                    slot.button.onClick.SetListener(() =>
                    {
                        player.inventory.CmdUseItem(inventoryIndex);
                    });

                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
#if !_iMMO2D
                    // use durability colors?
                    if (itemSlot.item.maxDurability > 0)
                    {
                        if (itemSlot.item.durability == 0)
                            slot.image.color = brokenDurabilityColor;
                        else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                            slot.image.color = lowDurabilityColor;
                        else
                            slot.image.color = Color.white;
                    }
                    else slot.image.color = Color.white; // reset for no-durability items
#else
                    slot.image.color = Color.white; // no-durability on 2d
#endif
                    slot.image.sprite = itemSlot.item.image;

                    slot.cooldownOverlay.SetActive(false);
                    // cooldown if usable item
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }

                /**
                 * Is a item in Equipment
                 */
                else if (equipmentIndex != -1)
                {
                    ItemSlot itemSlot = player.equipment.slots[equipmentIndex];

                    // refresh equipment slot
                    slot.button.onClick.RemoveAllListeners();
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;

#if !_iMMO2D
                    // use durability colors?
                    if (itemSlot.item.maxDurability > 0)
                    {
                        if (itemSlot.item.durability == 0)
                            slot.image.color = brokenDurabilityColor;
                        else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                            slot.image.color = lowDurabilityColor;
                        else
                            slot.image.color = Color.white;
                    }
                    else slot.image.color = Color.white; // reset for no-durability items
#else
                    slot.image.color = Color.white; // no-durability on 2d
#endif
                    slot.image.sprite = itemSlot.item.image;

                    slot.cooldownOverlay.SetActive(false);
                    // cooldown if usable item
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }

                /**
                 * Nothing = reset slot
                 */
                else
                {
                    // clear the outdated reference
                    // (need to assign directly because it's a struct)
                    player.skillbar.slots[i].reference = "";

                    // refresh empty slot
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                }
            }
        }
        else panel.SetActive(false);
    }

    public void activateCircleIndicator(Player player)
    {
        if (player.isLocalPlayer)
        {
            bs.CmdSetTargeting(true);
            bs.aoeCanvas.SetActive(true);
            Debug.Log("aoe canvas set true");
            bs.targetIndicator.enabled = true;
        }
    }

    public void clickTargeting(Player player)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, bs.raycastBlockingLayers))
        {
            if (hit.collider.gameObject != gameObject)
            {
                Vector3 hitPoint = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                Vector3 targetPoint = bs.targetIndicator.transform.position;

                // Send the mouse position to the server


                bs.CmdSetVector3(targetPoint); // Set the target point as before
            }
        }
        bs.setDefaultCursor();
        bs.targetIndicator.enabled = false;
        bs.aoeCanvas.SetActive(false);
        bs.CmdSetTargeting(false);
    }
#endif
    }

    public partial struct Skill
    {
        public bool isAOE => data.isAOE;
    }

    public partial class ScriptableSkill : ScriptableObject
    {
        public bool isAOE;
    }

    public partial class Mount
    {
        private void Awake()
        {
            animator.SetBool("OnGround", true);
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetBool("OnGround", true);
            }
        }
        // networkbehaviour ////////////////////////////////////////////////////////
        void Update()
        {
            if (isClient) // no need for animations on the server
            {
                if (owner == null)
                    return;

                if (!pccm) { pccm = owner.GetComponent<PlayerCharacterControllerMovement>(); }

                Vector3 playerVelocity = pccm.GetVelocity(); // This gets the player's velocity
                Vector3 localVelocity = transform.InverseTransformDirection(playerVelocity); // Convert velocity to local space
                CopyOwnerPositionAndRotation2();
                // Get all Animator components (on the object and its children)
                Animator[] animators = GetComponentsInChildren<Animator>();

                foreach (Animator anim in animators)
                {
                    // Update the animation parameters for each Animator
                    anim.SetFloat("DirX", localVelocity.x, .05f, Time.deltaTime); // Smooth transition for DirX
                    anim.SetFloat("DirY", localVelocity.y, .05f, Time.deltaTime); // Smooth transition for DirY
                    anim.SetFloat("DirZ", localVelocity.z, .05f, Time.deltaTime); // Smooth transition for DirZ
                    anim.SetBool("OnGround", pccm.controller.isGrounded); // Update OnGround status
                    anim.SetBool("MountedSwim", pccm.inWater); // Update MountedSwim state


                }

            }
            void CopyOwnerPositionAndRotation2()
            {
                if (owner != null)
                {
                    transform.position = owner.transform.position;
                    transform.rotation = owner.transform.rotation;
                }
            }
        }
    }

    public partial class PlayerInventory
    {
        MyEquipmentPreviewer previewer;

        public void CtrlClickPreview(EquipmentItem itemData)
        {
            if (previewer == null)
                previewer = FindFirstObjectByType<MyEquipmentPreviewer>();

            previewer.PreviewEquipmentItem(itemData);
        }




    }

    public partial class Appearancedatabase
    {
        MyEquipmentPreviewer previewer;

        public void CtrlClickPreview(EquipmentItem itemData)
        {
            if (previewer == null)
                previewer = FindFirstObjectByType<MyEquipmentPreviewer>();

            previewer.PreviewEquipmentItem(itemData);
        }




    }

    public partial class UIInventory
    {
        public static string LastClickedItemName = string.Empty;
        /*private void LateUpdate()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                if (panel.activeSelf)
                {

                    for (int i = 0; i < player.inventory.slots.Count; ++i)
                    {
                        int icopy = i;

                        //ItemSlot itemSlot = player.inventory.slots[icopy];
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            player.inventory.CtrlClickPreview(icopy);
                        }
                        /*else if (itemSlot.item.data is UsableItem usable &&
                                         usable.CanUse(player, icopy))
                        {
                            player.inventory.CmdUseItem(icopy);
                        }
                    }
                }
            }
        }*/
    }








    [System.Serializable]
    public class AppearanceData
    {
        public string umaRecipe;
        public int hairIndex;
        public int beardIndex;
        public int eyebrowIndex;
        public int tattooIndex;

        public string hairColor;
        public string beardColor;
        public string eyeColor;
        public string tattooColor;
    }

    public partial class Entity
    {
        public bool isElite;
        public bool isBoss;
    }


    public static class CompressionUtils
    {
        public static bool IsBase64String(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            s = s.Trim();
            // quick check if it looks like base64 (rough)
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        public static string CompressToBase64(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
                {
                    gzip.Write(inputBytes, 0, inputBytes.Length);
                }
                return Convert.ToBase64String(output.ToArray());
            }
        }

        public static string DecompressIfBase64(string base64Input)
        {
            try
            {
                byte[] compressedBytes = Convert.FromBase64String(base64Input);
                using (MemoryStream input = new MemoryStream(compressedBytes))
                using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
                using (MemoryStream output = new MemoryStream())
                {
                    gzip.CopyTo(output);
                    return Encoding.UTF8.GetString(output.ToArray());
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("Decompression failed: " + ex.Message);
                return base64Input; // fallback if it's not compressed
            }
        }


        public static string CompressString(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return System.Convert.ToBase64String(mso.ToArray());
            }
        }

        public static string DecompressString(string compressedBase64)
        {
            if (string.IsNullOrEmpty(compressedBase64))
                return "";

            try
            {
                byte[] compressed = Convert.FromBase64String(compressedBase64);
                using (var msi = new MemoryStream(compressed))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        gs.CopyTo(mso);
                    }
                    return Encoding.UTF8.GetString(mso.ToArray());
                }
            }
            catch (FormatException e)
            {
                Debug.LogError("DecompressString: Input string is not a valid Base64 string.");
                return compressedBase64; // fallback to raw string to avoid crash
            }
            catch (Exception e)
            {
                Debug.LogError($"DecompressString: Exception during decompression: {e.Message}");
                return compressedBase64; // fallback to raw string to avoid crash
            }
        }


    }

    public static class ChunkUtils
    {
        public static readonly int ChunkSize = 60000; // safe size under Mirror limit (~60 KB)

        // Split string into chunks
        public static string[] SplitStringIntoChunks(string str)
        {
            int totalChunks = (str.Length + ChunkSize - 1) / ChunkSize;
            string[] chunks = new string[totalChunks];

            for (int i = 0; i < totalChunks; i++)
            {
                int start = i * ChunkSize;
                int length = Math.Min(ChunkSize, str.Length - start);
                chunks[i] = str.Substring(start, length);
            }

            return chunks;
        }

        // Join chunks back to full string
        public static string JoinChunks(string[] chunks)
        {
            return string.Concat(chunks);
        }
    }




    [System.Serializable]
    public class character_appearance
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }  // <-- integer primary key

        public string character { get; set; } // unique character name or id if you want

        public string compressedDna { get; set; }

        public int hairIndex { get; set; }
        public int beardIndex { get; set; }
        public int eyebrowIndex { get; set; }
        public int tattooIndex { get; set; }

        public string eyeColor { get; set; }

        public string skinColor { get; set; }

        public string hairColor { get; set; }
    }
    [System.Serializable]
    public class character_costume
    {
        public string character { get; set; }
        public string type { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int costumeHash { get; set; }
        public string unlockedCostumes { get; set; }
    }



    public partial class Database
    {
        // Call this from your Connect() to create the appearance table
        public static character_appearance GetDefaultAppearance()
        {
            return new character_appearance
            {
                hairIndex = 0,
                beardIndex = 0,
                eyebrowIndex = 0,
                tattooIndex = 0,

                // Black hair = #000000FF (RGBA hex)
                hairColor = "000000FF",

                // Blue eyes = #0000FFFF
                eyeColor = "0000FFFF",

                // Light brown skin = #CC9966FF (approximate RGBA)
                skinColor = "FFFFFFFF",

                // Defaults for costume
                
            };
        }

        public void CreateAppearanceTable()
        {
            connection.CreateTable<character_appearance>();
        }

        public void CreateCostumeTable()
        {
            connection.CreateTable<character_costume>();
        }

        public void SaveCharacterAppearance(character_appearance appearance)
        {
            var existing = connection.Table<character_appearance>().FirstOrDefault(a => a.character == appearance.character);
            if (existing == null)
            {
                connection.Insert(appearance);
            }
            else
            {
                // Update standard appearance fields
                existing.compressedDna = appearance.compressedDna;
                existing.hairIndex = appearance.hairIndex;
                existing.beardIndex = appearance.beardIndex;
                existing.eyebrowIndex = appearance.eyebrowIndex;
                existing.tattooIndex = appearance.tattooIndex;
                existing.hairColor = appearance.hairColor;
                existing.eyeColor = appearance.eyeColor;
                existing.skinColor = appearance.skinColor;

                // Costume-related fields
               

                connection.Update(existing);
            }
        }

        public void SaveCostumeInventory(PlayerInventory inventory)
        {
            Debug.Log($"Saving CostumeInventory for {inventory.name}");
            connection.Execute("DELETE FROM character_costume WHERE character=? AND type='inventory'", inventory.name);

            for (int i = 0; i < inventory.slots.Count; ++i)
            {
                ItemSlot slot = inventory.slots[i];
                if (slot.amount > 0)
                {
                    Debug.Log($"Saving slot {i}, item {slot.item.name}, costumeHash {slot.item.costumeHash}");
                    connection.InsertOrReplace(new character_costume
                    {
                        character = inventory.name,
                        name = slot.item.name,
                        slot = i,
                        amount = slot.amount,
                        costumeHash = slot.item.costumeHash,
                        type = "inventory"
                    });
                }
            }
        }
        public void SaveCostumeEquipment(PlayerEquipment equipment)
        {
            connection.Execute("DELETE FROM character_costume WHERE character=? AND type='equipment'", equipment.name);

            for (int i = 0; i < equipment.slots.Count; ++i)
            {
                ItemSlot slot = equipment.slots[i];
                if (slot.amount >= 1)
                {
                    connection.InsertOrReplace(new character_costume
                    {
                        character = equipment.name,
                        name = slot.item.name,
                        slot = i,
                        amount = slot.amount,
                        costumeHash = slot.item.costumeHash,
                        type = "equipment"
                    });
                }
            }
        }
        public void LoadCostumeInventory(PlayerInventory inventory)
        {
            List<character_costume> rows = connection.Query<character_costume>(
                "SELECT * FROM character_costume WHERE character=? AND type='inventory'", inventory.name);

            

            foreach (character_costume row in rows)
            {
                if (ScriptableItem.All.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    Item item = new Item(itemData);
                    item.costumeHash = row.costumeHash;

                    int slotIndex = row.slot;
                    ItemSlot oldSlot = inventory.slots[slotIndex]; // Save previous item in slot
                    ItemSlot newSlot = new ItemSlot(item, row.amount); // New item slot

                    inventory.slots[slotIndex] = newSlot;

                    Debug.Log($"Loaded costumeHash {row.costumeHash} into inventory slot {slotIndex} for item {row.name}");

                    
                }
            }

            // Debug current slots after loading costume
            for (int i = 0; i < inventory.slots.Count; i++)
            {
                Debug.Log($"Slot {i} after costume load: item {inventory.slots[i].item.name}, costumeHash {inventory.slots[i].item.costumeHash}");
            }
        }
        public void LoadCostumeEquipment(PlayerEquipment equipment)
        {
            List<character_costume> rows = connection.Query<character_costume>(
                "SELECT * FROM character_costume WHERE character=? AND type='equipment'", equipment.name);

            foreach (character_costume row in rows)
            {
                if (ScriptableItem.All.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    Item item = new Item(itemData);
                    item.costumeHash = row.costumeHash;

                    equipment.slots[row.slot] = new ItemSlot(item, row.amount);
                }
            }

            // Refresh visuals for all equipment slots after loading
            for (int i = 0; i < equipment.slots.Count; i++)
            {
                equipment.RefreshLocation(i);
            }
        }










        public character_appearance LoadCharacterAppearance(string characterName)
        {
            Debug.Log("loadcharacterappearance");

            if (string.IsNullOrEmpty(characterName))
            {
                Debug.LogWarning("LoadCharacterAppearance called with null or empty characterName");
                return GetDefaultAppearance(); // return default if name is bad
            }

            if (connection == null)
            {
                Debug.LogError("Database connection is null!");
                return GetDefaultAppearance(); // fallback
            }

            try
            {
                character_appearance result = connection.Table<character_appearance>()
                    .FirstOrDefault(a => a.character == characterName);

                if (result == null)
                {
                    Debug.LogWarning($"No saved appearance found for {characterName}, using default.");
                    return GetDefaultAppearance();
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in LoadCharacterAppearance: {ex}");
                return GetDefaultAppearance();
            }
        }
    }

    public partial struct CharactersAvailableMsg
    {
        public void Load_UMA(List<Player> players)
        {
            Debug.Log("Load_UMA called! Applying UMA preview data...");
            for (int i = 0; i < players.Count; ++i)
            {
                var appearance = Database.singleton.LoadCharacterAppearance(players[i].name);
                if (appearance == null)
                    appearance = Database.GetDefaultAppearance();

                // update the struct in place
                var preview = characters[i];
                preview.hairIndex = appearance.hairIndex;
                preview.beardIndex = appearance.beardIndex;
                preview.eyebrowIndex = appearance.eyebrowIndex;
                preview.tattooIndex = appearance.tattooIndex;
                preview.compressedDna = appearance.compressedDna;
                preview.hairColor = appearance.hairColor;
                preview.skinColor = appearance.skinColor;
                preview.eyeColor = appearance.eyeColor;

                characters[i] = preview; // re-assign back
            }
        }
    }
    public static class CostumeType
    {
        public const string Inventory = "inventory";
        public const string Equipment = "equipment";
        public const string Unlocked = "unlocked";
    }

}
 







