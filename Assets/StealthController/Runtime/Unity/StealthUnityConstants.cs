using StealthController.Core;

namespace StealthController.Unity
{
    public static class StealthUnityConstants
    {
        #region Collision

        public const int OVERLAP_BUFFER_SIZE = 16;
        public const int DEFAULT_ENVIRONMENT_MASK = 1;
        public const int CAPSULE_DIRECTION_Y_AXIS = 1;

        #endregion

        #region Camera

        public const float OCCLUSION_CUTOUT_SOFTNESS = 0.4f;
        public const float OCCLUSION_CUTOUT_DEPTH_MARGIN = 0.5f;
        public const string OCCLUSION_CUTOUT_TARGET_PROPERTY = "_StealthCutoutTarget";
        public const string OCCLUSION_CUTOUT_RADIUS_PROPERTY = "_StealthCutoutRadius";
        public const string OCCLUSION_CUTOUT_SOFTNESS_PROPERTY = "_StealthCutoutSoftness";
        public const string OCCLUSION_CUTOUT_DEPTH_MARGIN_PROPERTY = "_StealthCutoutDepthMargin";

        #endregion

        #region Mission

        public const float MISSION_VOLUME_DEFAULT_WIDTH = 3f;
        public const float MISSION_VOLUME_DEFAULT_HEIGHT = 2f;
        public const float MISSION_VOLUME_DEFAULT_DEPTH = 3f;
        public const float MISSION_GIZMO_ARROW_LENGTH = 1f;

        #endregion

        #region Navigation

        public const int NAV_PATH_CORNER_BUFFER_SIZE = 64;

        #endregion

        #region Vision

        public const float VISION_CONE_ALPHA = 0.35f;

        #endregion

        #region Guard

        public const float GUARD_DEFAULT_WAYPOINT_WAIT = 1f;

        #endregion

        #region Surveillance Camera

        public const float SURVEILLANCE_FLOOR_PROBE_DISTANCE = 10f;
        public const float SURVEILLANCE_ALERT_RADIUS = 15f;
        public const float SURVEILLANCE_HEAD_PITCH = 30f;

        #endregion

        #region Animation

        public const string ANIMATOR_SPEED_PARAMETER = "Speed";
        public const string ANIMATOR_WALL_LATERAL_PARAMETER = "WallLateral";
        public const string ANIMATOR_STAND_TO_CROUCH_PROGRESS_PARAMETER = "StandToCrouchProgress";
        public const string ANIMATOR_CROUCH_TO_STAND_PROGRESS_PARAMETER = "CrouchToStandProgress";
        public const string ANIMATOR_CROUCH_TO_PRONE_PROGRESS_PARAMETER = "CrouchToProneProgress";
        public const string ANIMATOR_PRONE_TO_CROUCH_PROGRESS_PARAMETER = "ProneToCrouchProgress";
        public const string ANIMATOR_STANDING_STATE = "Standing";
        public const string ANIMATOR_CROUCHED_STATE = "Crouched";
        public const string ANIMATOR_PRONE_STATE = "Prone";
        public const string ANIMATOR_WALL_STANDING_STATE = "WallStanding";
        public const string ANIMATOR_STAND_TO_CROUCH_STATE = "StandToCrouch";
        public const string ANIMATOR_CROUCH_TO_STAND_STATE = "CrouchToStand";
        public const string ANIMATOR_CROUCH_TO_PRONE_STATE = "CrouchToProne";
        public const string ANIMATOR_PRONE_TO_CROUCH_STATE = "ProneToCrouch";
        public const string ANIMATOR_LOCOMOTION_STATE = "Locomotion";
        public const string ANIMATOR_SEARCH_STATE = "Search";
        public const string ANIMATOR_TAKEDOWN_PROGRESS_PARAMETER = "TakedownProgress";
        public const string ANIMATOR_TAKEDOWN_STATE = "Takedown";
        public const string ANIMATOR_TAKEN_DOWN_STATE = "TakenDown";
        public const float ANIMATION_CROSSFADE_DURATION = 0.2f;
        public const float ANIMATION_PARAMETER_DAMP_TIME = 0.1f;

        #endregion

        #region Input

        public const string INPUT_GROUP_KEYBOARD = "Keyboard";
        public const string INPUT_GROUP_MOUSE = "Mouse";
        public const string INPUT_GROUP_GAMEPAD = "Gamepad";
        public const string INPUT_GAMEPAD_PATH_PREFIX = "<Gamepad>";
        public const string INPUT_CONTROL_LEFT_SHIFT = "leftShift";
        public const string INPUT_CONTROL_RIGHT_SHIFT = "rightShift";
        public const string INPUT_CONTROL_LEFT_CTRL = "leftCtrl";
        public const string INPUT_CONTROL_RIGHT_CTRL = "rightCtrl";
        public const string INPUT_CONTROL_LEFT_ALT = "leftAlt";
        public const string INPUT_CONTROL_LEFT_STICK_PRESS = "leftStickPress";
        public const string INPUT_CONTROL_RIGHT_STICK_PRESS = "rightStickPress";
        public const string INPUT_CONTROL_LEFT_TRIGGER = "leftTrigger";
        public const string INPUT_CONTROL_RIGHT_TRIGGER = "rightTrigger";
        public const string INPUT_KEY_LABEL_SHIFT = "Shift";
        public const string INPUT_KEY_LABEL_CTRL = "Ctrl";
        public const string INPUT_KEY_LABEL_ALT = "Alt";
        public const string INPUT_KEY_LABEL_LEFT_STICK = "LS";
        public const string INPUT_KEY_LABEL_RIGHT_STICK = "RS";
        public const string INPUT_KEY_LABEL_LEFT_TRIGGER = "LT";
        public const string INPUT_KEY_LABEL_RIGHT_TRIGGER = "RT";

        #endregion

        #region Texts

        public const int TEXT_FALLBACK_LANGUAGE_INDEX = 0;
        public const string LANGUAGE_CODE_ENGLISH = "en";
        public const string LANGUAGE_CODE_SPANISH = "es";
        public const string TEXT_KEY_POSTURE_STANDING = "posture.standing";
        public const string TEXT_KEY_POSTURE_CROUCHED = "posture.crouched";
        public const string TEXT_KEY_POSTURE_PRONE = "posture.prone";
        public const string TEXT_KEY_ACTION_SPRINT = "action.sprint";
        public const string TEXT_KEY_ACTION_STAND = "action.stand";
        public const string TEXT_KEY_ACTION_CROUCH = "action.crouch";
        public const string TEXT_KEY_ACTION_PRONE = "action.prone";
        public const string TEXT_KEY_ACTION_HOLD = "action.hold";
        public const string TEXT_KEY_OUTCOME_CAPTURED = "outcome.captured";
        public const string TEXT_KEY_OUTCOME_ZONE_CLEARED = "outcome.zoneCleared";
        public const string TEXT_KEY_PAUSE_TITLE = "pause.title";
        public const string TEXT_KEY_PAUSE_RESUME = "pause.resume";
        public const string TEXT_KEY_PAUSE_RESTART = "pause.restart";
        public const string TEXT_KEY_PAUSE_EXIT = "pause.exit";

        #endregion

        #region UI

        public const float UI_SHOW_DURATION = 0.2f;
        public const float UI_HIDE_DURATION = 0.1f;
        public const float OUTCOME_BANNER_HOLD_DURATION = 1.5f;
        public const float POSTURE_HUD_PUNCH_SCALE = 0.12f;
        public const float POSTURE_HUD_PUNCH_DURATION = 0.25f;
        public const float TAKEDOWN_PROMPT_HEIGHT = 0.55f;

        #endregion
    }
}
