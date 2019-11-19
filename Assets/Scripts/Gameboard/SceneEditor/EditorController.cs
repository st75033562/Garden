using DataAccess;
using RobotSimulation;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameboard
{
    public class EditorController : MonoBehaviour, IObjectDragHandler
    {
        public Editor m_editor;
        public UIObjectMenu m_objectMenu;
        public GameObject m_optionButton;
        public UIAssetListView m_assetListView;
        public UIObjectSettingsDialog m_objectSettingsDialog;
        public GameboardSceneManager m_sceneManager;
        public MoveTool m_moveTool;
        public GizmoManager m_gizmoManager;

        private AsyncRequest<int> m_createObjectRequest;

        private Entity m_draggingEntity;

        private Vector3 m_oldGravity;
        private bool m_editing;

        private bool m_menuOpen;

        private Vector3 m_oldEntityPosition;
        private FreeMoveController m_freeMoveController;

        private UIWorkspace m_workspace;

        private readonly InputManager m_inputManager = new InputManager();
        private Gameboard m_gameboard;

        private UndoManager m_undoManager;
        private int m_prevEntityId;

        private ObjectNameGenerator m_objNameGen;

        private const float DefaultDistanceToCamera = 10.0f;
        private static readonly Vector3 OptionButtonOffset = new Vector3(1, 1, 0);

        private const float MoveToolMobileExtraScale = 1.5f;
        private const float MoveToolMobileLineHitDistance = 10.0f;
        private const float MoveToolMobileConeHitRadius = 20.0f;

        public void Initialize(UIWorkspace workspace, UndoManager undoManager)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException("workspace");
            }

            if (undoManager == null)
            {
                throw new ArgumentNullException("undoManager");
            }

            m_workspace = workspace;
            m_workspace.deletingVariableHandler = OnDeletingVariable;

            m_undoManager = undoManager;

            m_editor.onBeforeChangingSelection += OnBeforeChangingSelection;
            m_editor.onSelectionChanged += OnSelectionChanged;
            m_assetListView.dragHandler = this;

            m_inputManager.Register(m_moveTool);

            m_sceneManager.onEndLoading.AddListener(OnSceneLoaded);
            m_sceneManager.objectManager.onEntityActivated += OnEntityActivated;
            m_sceneManager.objectManager.onEntityRemoved += OnEntityRemoved;

            inputEnabled = true;

            if (Application.isMobilePlatform)
            {
                m_moveTool.scale = MoveToolMobileExtraScale;
                m_moveTool.lineHitDistance = MoveToolMobileLineHitDistance;
                m_moveTool.coneHitRadius = MoveToolMobileConeHitRadius;
            }
        }

        public void SetGameboard(Gameboard gameboard)
        {
            if (gameboard == null)
            {
                throw new ArgumentNullException("gameboard");
            }

            m_gameboard = gameboard;
            m_objNameGen = new ObjectNameGenerator(m_gameboard, m_workspace.CodeContext.variableManager);
        }

        private void OnDeletingVariable(string name, Action delete)
        {
            var objInfo = m_gameboard.GetObject(name);
            if (objInfo != null)
            {
                PopupManager.YesNo("ui_deleting_var_will_delete_object".Localize(), () => {
                    m_undoManager.BeginMacro(UndoContext.Any);

                    DeleteEntity(name, false);
                    delete();

                    m_undoManager.EndMacro();
                });
            }
            else
            {
                delete();
            }
        }

        private void OnVariableRemoved(BaseVariable data)
        {
            var objInfo = m_gameboard.GetObject(data.name);
            if (objInfo != null)
            {
                m_gameboard.RemoveObject(objInfo);

                var entity = m_sceneManager.objectManager.Get(data.name);
                Destroy(entity.gameObject);

                if (selectedEntity == entity)
                {
                    selectedEntity = null;
                }
            }
        }

        private void OnBeforeChangingSelection()
        {
            if (selectedEntity)
            {
                Editor.EnablePlacementErrorDetection(selectedEntity, false);
            }
        }

        private void OnSelectionChanged()
        {
            if (selectedEntity)
            {
                Editor.EnablePlacementErrorDetection(selectedEntity, true);
                m_moveTool.target = selectedEntity.transform;
                m_moveTool.renderCamera = m_sceneManager.currentCamera;
            }
            else
            {
                m_moveTool.target = null;
            }

            UpdateMoveTool();
            UpdateOptionButton();

            if (m_menuOpen)
            {
                m_objectMenu.Close();
            }
        }

        private void OnSceneLoaded()
        {
            m_gizmoManager.worldCamera = m_sceneManager.currentCamera;
        }

        private void OnEntityActivated(Entity entity)
        {
            if (m_editing)
            {
                m_editor.SetupEntity(entity, false);
            }
        }

        private void OnEntityRemoved(Entity entity)
        {
            m_gizmoManager.RemoveGizmo(entity.transform);
            if (selectedEntity == entity)
            {
                selectedEntity = null;
            }
        }

        private void UpdateOptionButton()
        {
            m_optionButton.gameObject.SetActive(selectedEntity != null);
        }

        private void UpdateMoveTool()
        {
            m_moveTool.gameObject.SetActive(selectedEntity != null);
        }

        public void StartEditing()
        {
            if (m_editing)
            {
                return;
            }

            foreach (var entity in m_sceneManager.objectManager.objects)
            {
                m_editor.SetupEntity(entity, false);
            }

            m_editing = true;
            m_oldGravity = Physics.gravity;
            Physics.gravity = Vector3.zero;
        }

        public void StopEditing()
        {
            if (m_editing)
            {
                m_editing = false;
                Physics.gravity = m_oldGravity;

                m_gizmoManager.RemoveGizmos();
            }

            selectedEntity = null;
        }

        public bool inputEnabled { get; set; }

        void LateUpdate()
        {
            if (selectedEntity)
            {
                m_moveTool.transform.position = selectedEntity.transform.position;
            }

            PositionOptionButton();
            PositionObjectMenu();
        }

        void OnDestroy()
        {
            StopEditing();
        }

        public Entity selectedEntity
        {
            get { return m_editor.selectedEntity; }
            set { m_editor.selectedEntity = value; }
        }

        public void OpenObjectMenu()
        {
            if (selectedEntity == null)
            {
                Debug.Log("no entity is selected");
                return;
            }

            if (!m_menuOpen)
            {
                m_menuOpen = true;
                m_objectMenu.Open(OnObjectMenuClosed);
            }
            else
            {
                m_objectMenu.Close();
            }
        }

        void OnObjectMenuClosed()
        {
            m_menuOpen = false;
        }

        void PositionOptionButton()
        {
            if (m_optionButton.gameObject.activeInHierarchy)
            {
                var worldCamera = m_sceneManager.currentCamera;
                var position = m_moveTool.transform.position + worldCamera.transform.rotation * OptionButtonOffset;
                m_optionButton.transform.position = worldCamera.WorldToScreenPoint(position);
            }
        }

        void PositionObjectMenu()
        {
            if (m_menuOpen)
            {
                var optionTrans = m_optionButton.GetComponent<RectTransform>();
                var objectMenuTrans = m_objectMenu.GetComponent<RectTransform>();

                objectMenuTrans.Align(UIAnchor.BottomCenter, Vector2.zero, optionTrans, UIAnchor.UpperCenter);
                objectMenuTrans.RestrictWithinCanvas(UIRestrictAxis.X);
            }
        }

        private VariableManager varManager
        {
            get { return m_workspace.CodeContext.variableManager; }
        }

        public void OpenSettingsDialog()
        {
            if (selectedEntity == null)
            {
                Debug.Log("no entity is selected");
                return;
            }

            m_objectMenu.Close();

            IObjectPropertyViewModel viewModel;
            var robot = selectedEntity.GetComponent<Robot>();
            if (robot)
            {
                var robotInfo = m_gameboard.robots[robot.robotIndex];
                var undo = new UndoRobotSettings(m_undoManager, m_sceneManager.objectManager, m_gameboard);
                viewModel = new RobotPropertyViewModel(robot, robotInfo, undo);
            }
            else
            {
                var objectInfo = m_gameboard.GetObject(selectedEntity.entityName);
                var validator = new ObjectNameValidator(objectInfo.name, m_gameboard, varManager);
                var undo = new UndoObjectSettings(
                    m_undoManager, m_sceneManager.objectManager, m_gameboard, m_workspace.CodeContext.variableManager);
                viewModel = new ObjectPropertyViewModel(selectedEntity, objectInfo, validator, undo);
            }

            m_objectSettingsDialog.Open();
            m_objectSettingsDialog.SetViewModel(viewModel);
        }

        public void DeleteSelectedEntity()
        {
            if (selectedEntity == null)
            {
                Debug.Log("no entity is selected");
                return;
            }

            DeleteEntity(selectedEntity);
        }

        private void DeleteEntity(Entity entity)
        {
            var robot = entity.GetComponent<Robot>();
            if (robot)
            {
                m_undoManager.AddUndo(new DeleteRobotCommand(m_editor, m_sceneManager, robot.robotIndex));
            }
            else
            {
                m_undoManager.BeginMacro(UndoContext.Gameboard);
                DeleteEntity(entity.entityName, true);
                m_undoManager.EndMacro();
            }

            if (selectedEntity == entity)
            {
                selectedEntity = null;
            }
        }

        private void DeleteEntity(string entityName, bool deleteVar)
        {
            var entity = m_sceneManager.objectManager.Get(entityName);
            if (entity)
            {
                var objInfo = m_sceneManager.gameboard.GetObject(entityName);
                m_undoManager.AddUndo(new DeleteEntityCommand(m_sceneManager, entity.id, objInfo));
            }

            if (deleteVar)
            {
                m_workspace.UndoManager.AddUndo(new DeleteVariableCommand(m_workspace, entityName));
            }
        }

        public void OnPointerDown(BaseEventData eventData)
        {
            if (!m_editing || !inputEnabled) { return; }

            var pointerEventData = (PointerEventData)eventData;

            // first check if any manipulators are hit
            if (!m_inputManager.OnPointerDown(pointerEventData.position))
            {
                selectedEntity = GetEntity(pointerEventData.position);

                if (selectedEntity)
                {
                    // hide the move tool while freely dragging
                    m_moveTool.gameObject.SetActive(false);
                    HideOptionButtonAndMenu();

                    m_freeMoveController = new FreeMoveController(selectedEntity, Camera.main, pointerEventData.position);
                }
            }
            else
            {
                HideOptionButtonAndMenu();
            }

            if (selectedEntity)
            {
                m_oldEntityPosition = selectedEntity.transform.position;
            }
        }

        private void HideOptionButtonAndMenu()
        {
            m_optionButton.gameObject.SetActive(false);
            if (m_menuOpen)
            {
                m_objectMenu.Close();
            }
        }

        public void OnPointerUp(BaseEventData eventData)
        {
            m_inputManager.OnPointerUp();
            m_freeMoveController = null;

            if (selectedEntity)
            {
                if (selectedEntity.transform.position != m_oldEntityPosition)
                {
                    IObjectInfo objInfo;
                    var robot = selectedEntity.GetComponent<Robot>();
                    if (robot)
                    {
                        objInfo = m_gameboard.robots[robot.robotIndex];
                    }
                    else
                    {
                        objInfo = m_gameboard.GetObject(selectedEntity.entityName);
                    }
                    m_gameboard.isDirty = true;

                    AddMoveCommand(objInfo);
                }

                UpdateMoveTool();
                UpdateOptionButton();
            }
        }

        private void AddMoveCommand(IObjectInfo objInfo)
        {
            var cmd = new MoveEntityCommand(
                m_sceneManager.objectManager, 
                selectedEntity.id, 
                objInfo, 
                m_oldEntityPosition, 
                selectedEntity.transform.position);
            m_undoManager.AddUndo(cmd);
        }

        public void OnDrag(BaseEventData eventData)
        {
            var pointerEventData = (PointerEventData)eventData;

            if (m_inputManager.OnDrag(pointerEventData.position))
            {
                return;
            }

            if (m_freeMoveController != null)
            {
                m_freeMoveController.OnDrag(pointerEventData.position);
                return;
            }
        }

        private Entity GetEntity(Vector2 inputPosition)
        {
            var ray = Camera.main.ScreenPointToRay(inputPosition);
            RaycastHit hit;

            var mask = Physics.DefaultRaycastLayers & ~(1 << PhysicsUtils.PlacementLayer);
            mask &= ~(1 << PhysicsUtils.IgnoreEditorPicking);
            if (Physics.Raycast(ray, out hit, float.MaxValue, mask, QueryTriggerInteraction.Collide))
            {
                return hit.transform.GetComponentInParent<Entity>();
            }
            else
            {
                return null;
            }
        }

        void IObjectDragHandler.OnBeginDrag(BundleAssetData asset, Vector2 pointerPos)
        {
            m_assetListView.Show(false);
            StartCoroutine(CreateDragObject(asset, pointerPos));
        }

        private IEnumerator CreateDragObject(BundleAssetData asset, Vector2 pointerPos)
        {
            // save the seed in case the object is destroyed before drag finishes
            m_prevEntityId = m_sceneManager.objectManager.prevEntityId;
            var position = GetObjectPosition(pointerPos);
            m_createObjectRequest = m_sceneManager.objectFactory.Create(
                new ObjectCreateInfo {
                    assetId = asset.id,
                    position = position,
                });
            yield return m_createObjectRequest;

            if (m_createObjectRequest.result != 0)
            {
                m_draggingEntity = m_sceneManager.objectManager.Get(m_createObjectRequest.result);
                Editor.EnablePlacementErrorDetection(m_draggingEntity, true);
            }
            m_createObjectRequest = null;
        }

        void IObjectDragHandler.OnDrag(Vector2 pointerPos)
        {
            if (m_draggingEntity)
            {
                m_draggingEntity.transform.position = GetObjectPosition(pointerPos);
            }
        }

        Vector3 GetObjectPosition(Vector2 pointerPos)
        {
            var camera = m_sceneManager.currentCamera;
            var ray = camera.ScreenPointToRay(pointerPos);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, float.MaxValue, 1 << PhysicsUtils.PlacementLayer))
            {
                return hitInfo.point;
            }

            return camera.transform.position + DefaultDistanceToCamera * camera.transform.forward;
        }

        void IObjectDragHandler.OnEndDrag(Vector2 pointerPos)
        {
            StopAllCoroutines();
            if (m_createObjectRequest != null)
            {
                m_createObjectRequest.Dispose();
                m_createObjectRequest = null;
                m_sceneManager.objectManager.prevEntityId = m_prevEntityId;
            }

            if (m_draggingEntity)
            {
                var assetInfo = m_gameboard.GetAssetInfo(m_draggingEntity.asset.id);
                var oldNextObjNum = assetInfo.nextObjectNum;

                var objectInfo = new ObjectInfo(assetInfo.assetId);
                objectInfo.name = m_objNameGen.Generate(assetInfo);
                objectInfo.position = m_draggingEntity.transform.position;
                m_draggingEntity.entityName = objectInfo.name;

                m_gameboard.AddObject(objectInfo);

                m_draggingEntity.positional.Synchornize();

                selectedEntity = m_draggingEntity;
                m_draggingEntity = null;

                RecordAddEntity(oldNextObjNum, assetInfo.nextObjectNum, objectInfo);
            }
        }

        void RecordAddEntity(int oldNextObjNum, int newNextObjNum, ObjectInfo objectInfo)
        {
            m_undoManager.BeginMacro(UndoContext.Gameboard);

            var addEntityCmd = new AddEntityCommand(
                m_sceneManager,
                oldNextObjNum,
                newNextObjNum,
                objectInfo,
                m_gameboard.objects.Count - 1);
            addEntityCmd.prevEntityId = m_prevEntityId;
            m_undoManager.AddUndo(addEntityCmd, false);

            var addVarCmd = new AddVariablesCommand(m_workspace, new VariableData(objectInfo.name, NameScope.Local));
            m_undoManager.AddUndo(addVarCmd);

            m_undoManager.EndMacro();
        }
    }
}
