namespace StealthController.Core
{
    public static class StealthConstants
    {
        #region Math

        public const float MIN_MOVE_DISTANCE = 0.0001f;
        public const float FULL_STICK_TILT = 1f;
        public const float RIGHT_ANGLE_DEGREES = 90f;
        public const float HALF_TURN_DEGREES = 180f;
        public const float FULL_TURN_DEGREES = 360f;
        public const float NEAR_VERTICAL_DEGREES = 89f;

        #endregion

        #region Player Input

        public const float PLAYER_STICK_DEAD_ZONE = 0.2f;
        public const float PLAYER_MAX_STICK_DEAD_ZONE = 0.9f;
        public const float PLAYER_RUN_INPUT_THRESHOLD = 0.7f;
        public const float PLAYER_MIN_RUN_INPUT_THRESHOLD = 0.1f;

        #endregion

        #region Player Movement

        public const float PLAYER_MIN_WALK_SPEED = 0.6f;
        public const float PLAYER_MAX_WALK_SPEED = 1.6f;
        public const float PLAYER_RUN_SPEED = 4.2f;
        public const float PLAYER_CROUCH_WALK_MIN_SPEED = 0.4f;
        public const float PLAYER_CROUCH_WALK_SPEED = 1f;
        public const float PLAYER_CRAWL_SPEED = 0.9f;
        public const float PLAYER_ACCELERATION = 18f;
        public const float PLAYER_DECELERATION = 24f;
        public const float PLAYER_STANDING_TURN_SPEED = 900f;
        public const float PLAYER_CRAWLING_TURN_SPEED = 360f;
        public const float PLAYER_STANDING_TURN_SMOOTH_TIME = 0.08f;
        public const float PLAYER_CRAWLING_TURN_SMOOTH_TIME = 0.2f;

        #endregion

        #region Player Posture

        public const float PLAYER_POSTURE_TRANSITION_DURATION = 0.2f;
        public const float PLAYER_PRONE_TRANSITION_DURATION = 0.5f;
        public const float PLAYER_CROUCH_HOLD_TO_PRONE_DURATION = 0.4f;
        public const float PLAYER_STANDING_RADIUS = 0.3f;
        public const float PLAYER_CRAWLING_RADIUS = 0.25f;
        public const float PLAYER_MIN_CAPSULE_RADIUS = 0.05f;
        public const float PLAYER_STANDING_HEIGHT = 1.8f;
        public const float PLAYER_CROUCHED_HEIGHT = 1.1f;
        public const float PLAYER_CRAWLING_HEIGHT = 0.5f;
        public const float PLAYER_MIN_CAPSULE_HEIGHT = 0.1f;

        #endregion

        #region Wall

        public const float WALL_DETECT_DISTANCE = 0.35f;
        public const float WALL_ANGLE_TOLERANCE = 40f;
        public const float WALL_PUSH_DURATION = 0.15f;
        public const float WALL_STANDING_MIN_HEIGHT = 1.5f;
        public const float WALL_MIN_HEIGHT_LIMIT = 0.1f;
        public const float WALL_SHUFFLE_STANDING_SPEED = 1.3f;
        public const float WALL_LATERAL_INPUT_THRESHOLD = 0.35f;
        public const float WALL_LEAVE_INPUT_THRESHOLD = 0.75f;
        public const float WALL_CORNER_ANTICIPATION_DISTANCE = 0.6f;
        public const float WALL_CORNER_DEPTH_PROBE = 0.1f;
        public const float WALL_CORNER_MIN_TURN_ANGLE = 30f;
        public const float WALL_PROBE_HEIGHT_RATIO = 0.5f;
        public const float WALL_HEIGHT_CHECK_MARGIN = 0.05f;
        public const float WALL_REACQUIRE_MAX_ANGLE = 45f;

        #endregion

        #region Collision

        public const float COLLISION_SKIN_WIDTH = 0.015f;
        public const float COLLISION_MIN_SKIN_WIDTH = 0.001f;
        public const int COLLISION_SLIDE_ITERATIONS = 4;
        public const int COLLISION_MIN_SLIDE_ITERATIONS = 1;
        public const int COLLISION_MAX_SLIDE_ITERATIONS = 8;
        public const float COLLISION_STEP_OFFSET = 0.3f;
        public const float COLLISION_SLOPE_LIMIT = 45f;
        public const float COLLISION_GRAVITY = 20f;
        public const float COLLISION_MAX_FALL_SPEED = 20f;
        public const float COLLISION_GROUND_SNAP_DISTANCE = 0.3f;
        public const float SHAPE_CHECK_FLOOR_CLEARANCE_SKINS = 2f;
        public const float GROUND_EDGE_PROBE_INSET = 0.02f;
        public const float GROUND_EDGE_PROBE_HEIGHT = 0.05f;
        public const float GROUND_EDGE_PROBE_LENGTH = 0.1f;

        #endregion

        #region Camera

        public const float CAMERA_DEFAULT_PITCH = 60f;
        public const float CAMERA_DEFAULT_YAW = 0f;
        public const float CAMERA_DEFAULT_DISTANCE = 14f;
        public const float CAMERA_DEFAULT_FIELD_OF_VIEW = 35f;
        public const float CAMERA_MIN_DISTANCE = 0.5f;
        public const float CAMERA_MIN_FIELD_OF_VIEW = 5f;
        public const float CAMERA_MAX_FIELD_OF_VIEW = 120f;
        public const float CAMERA_PIVOT_HEIGHT = 1f;
        public const float CAMERA_LOOK_AHEAD_DISTANCE = 0.8f;
        public const float CAMERA_FOLLOW_SMOOTH_TIME = 0.15f;
        public const float CAMERA_MIN_SMOOTH_TIME = 0.01f;
        public const float CAMERA_OCCLUSION_MIN_TARGET_HEIGHT = 0.6f;
        public const float OCCLUSION_CUTOUT_RADIUS = 1.4f;
        public const float OCCLUSION_CUTOUT_OPEN_SPEED = 6f;
        public const float OCCLUSION_CUTOUT_CLOSE_SPEED = 3f;

        #endregion

        #region Vision

        public const float VISION_CONFIRM_DURATION = 0.25f;
        public const float VISION_HEAD_HEIGHT_RATIO = 0.9f;
        public const float VISION_CHEST_HEIGHT_RATIO = 0.6f;
        public const float VISION_FEET_HEIGHT_RATIO = 0.1f;
        public const float VISION_LINE_OF_SIGHT_TOLERANCE = 0.05f;
        public const int VISION_CONE_DEFAULT_SEGMENTS = 48;
        public const int VISION_CONE_EDGE_ITERATIONS = 6;
        public const int VISION_CONE_MAX_EDGE_ITERATIONS = 12;
        public const float VISION_CONE_EDGE_DISTANCE_THRESHOLD = 0.5f;
        public const int VISION_CONE_MIN_SEGMENTS = 4;
        public const int VISION_CONE_MAX_SEGMENTS = 96;
        public const float VISION_CONE_FLOOR_OFFSET = 0.06f;
        public const float VISION_CONE_MIN_GROUND_NORMAL_Y = 0.2f;
        public const int VISION_CONE_DEFAULT_RINGS = 6;
        public const int VISION_CONE_MAX_RINGS = 16;
        public const float VISION_CONE_GROUND_PROBE_HEIGHT = 1f;
        public const float VISION_CONE_GROUND_PROBE_DISTANCE = 6f;
        public const float VISION_CONE_GROUND_CELL_SIZE = 0.25f;
        public const float VISION_CONE_GROUND_HEIGHT_BAND = 1f;
        public const float VISION_CONE_MAX_GROUND_RISE = 0.5f;

        #endregion

        #region Guard

        public const float GUARD_VISION_RANGE = 9f;
        public const float GUARD_VISION_ANGLE = 60f;
        public const float GUARD_EYE_HEIGHT = 1.6f;
        public const float GUARD_PATROL_SPEED = 1.4f;
        public const float GUARD_CHASE_SPEED = 3.8f;
        public const float GUARD_TURN_SPEED = 360f;
        public const float GUARD_TURN_SMOOTH_TIME = 0.12f;
        public const float GUARD_ACCELERATION = 10f;
        public const float GUARD_CAPTURE_DISTANCE = 0.8f;
        public const float GUARD_REPATH_INTERVAL = 0.25f;
        public const float GUARD_SEARCH_DURATION = 4f;
        public const float GUARD_SEARCH_SWEEP_ANGLE = 90f;
        public const float GUARD_SEARCH_SWEEP_SPEED = 120f;
        public const float GUARD_WALKABLE_SNAP_DISTANCE = 2f;

        #endregion

        #region Surveillance Camera

        public const float SURVEILLANCE_VISION_RANGE = 10f;
        public const float SURVEILLANCE_VISION_ANGLE = 50f;
        public const float SURVEILLANCE_VISION_MIN_RANGE = 1.8f;
        public const float SURVEILLANCE_SIGHT_HEIGHT = 1.6f;
        public const float SURVEILLANCE_SWEEP_HALF_ANGLE = 45f;
        public const float SURVEILLANCE_SWEEP_SPEED = 20f;
        public const float SURVEILLANCE_SWEEP_PAUSE_DURATION = 1.5f;
        public const float SURVEILLANCE_ALERT_COOLDOWN = 5f;

        #endregion

        #region Takedown

        public const float TAKEDOWN_MAX_DISTANCE = 2f;
        public const float TAKEDOWN_MAX_HEIGHT_DIFFERENCE = 0.5f;
        public const float TAKEDOWN_BACK_HALF_ANGLE = 60f;
        public const float TAKEDOWN_ATTACKER_OFFSET = 1.85f;
        public const float TAKEDOWN_APPROACH_SPEED = 0.97f;
        public const float TAKEDOWN_MAX_START_TIME = 1.25f;
        public const float TAKEDOWN_CLIP_LENGTH = 7f;
        public const float TAKEDOWN_END_TIME = 5.5f;
        public const float TAKEDOWN_EXIT_OFFSET_SIDE = -0.17f;
        public const float TAKEDOWN_EXIT_OFFSET_FORWARD = 1.89f;
        public const float TAKEDOWN_EXIT_YAW_OFFSET = -13f;
        public const float TAKEDOWN_CHEST_HEIGHT_RATIO = 0.6f;
        public const float TAKEDOWN_MAX_ATTACKER_OFFSET = 4f;
        public const float TAKEDOWN_MAX_CLIP_LENGTH = 30f;

        #endregion
    }
}
