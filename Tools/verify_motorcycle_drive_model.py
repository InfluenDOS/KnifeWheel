#!/usr/bin/env python3
"""Smoke-check mirror of MotorcycleDriveModel behaviors for environments without Unity.

This is not a substitute for Unity EditMode/PlayMode tests. It only verifies that the
documented arcade-drive rules remain internally consistent when Unity Test Runner
cannot run in the current agent environment.
"""

from __future__ import annotations

import math
import sys


class Settings:
    acceleration = 20.0
    max_speed = 10.0
    brake_deceleration = 30.0
    coast_deceleration = 2.0
    steer_yaw_acceleration = 6.0
    max_steer_yaw_rate = 2.0
    min_speed_for_steer = 0.5
    full_steer_speed = 3.0


EPS = 1e-4


def longitudinal_accel(forward_speed: float, throttle: float, brake: float, s: Settings) -> float:
    throttle = max(0.0, min(1.0, throttle))
    brake = max(0.0, min(1.0, brake))
    accel = 0.0

    if throttle > 0.0 and forward_speed < s.max_speed:
        headroom = s.max_speed - max(forward_speed, 0.0)
        throttle_accel = throttle * s.acceleration
        limit_factor = max(0.0, min(1.0, headroom / max(s.max_speed * 0.05, 0.01)))
        accel += throttle_accel * limit_factor

    if brake > 0.0:
        if abs(forward_speed) > EPS:
            accel += -math.copysign(1.0, forward_speed) * brake * s.brake_deceleration
    elif throttle <= 0.0 and abs(forward_speed) > EPS:
        accel += -math.copysign(1.0, forward_speed) * s.coast_deceleration

    return accel


def steer_effectiveness(forward_speed: float, s: Settings) -> float:
    abs_speed = abs(forward_speed)
    if abs_speed < s.min_speed_for_steer:
        return 0.0
    span = s.full_steer_speed - s.min_speed_for_steer
    if span <= EPS:
        return 1.0
    return max(0.0, min(1.0, (abs_speed - s.min_speed_for_steer) / span))


def yaw_accel(forward_speed: float, steer: float, s: Settings) -> float:
    steer = max(-1.0, min(1.0, steer))
    effectiveness = steer_effectiveness(forward_speed, s)
    if effectiveness <= 0.0 or abs(steer) <= EPS:
        return 0.0
    direction = -1.0 if forward_speed < 0.0 else 1.0
    return steer * direction * s.steer_yaw_acceleration * effectiveness


def clamp_yaw(yaw: float, s: Settings) -> float:
    return max(-s.max_steer_yaw_rate, min(s.max_steer_yaw_rate, yaw))


def check(condition: bool, message: str, failures: list[str]) -> None:
    if not condition:
        failures.append(message)


def main() -> int:
    s = Settings()
    failures: list[str] = []

    check(longitudinal_accel(0.0, 1.0, 0.0, s) > 0.0, "throttle should accelerate", failures)
    check(longitudinal_accel(5.0, 0.0, 1.0, s) < 0.0, "brake should decelerate", failures)
    check(abs(longitudinal_accel(0.0, 0.0, 1.0, s)) <= EPS, "brake at rest is zero", failures)
    check(abs(longitudinal_accel(s.max_speed, 1.0, 0.0, s)) <= EPS, "no throttle accel at max", failures)
    check(steer_effectiveness(0.0, s) == 0.0, "no steer at standstill", failures)
    check(abs(yaw_accel(0.0, 1.0, s)) <= EPS, "no yaw at standstill", failures)
    check(yaw_accel(5.0, 1.0, s) > 0.0, "yaw follows steer at speed", failures)
    check(
        abs(yaw_accel(5.0, 1.0, s) + yaw_accel(-5.0, 1.0, s)) <= EPS,
        "reverse inverts steer yaw",
        failures,
    )
    check(clamp_yaw(99.0, s) == s.max_steer_yaw_rate, "yaw clamp high", failures)
    check(clamp_yaw(-99.0, s) == -s.max_steer_yaw_rate, "yaw clamp low", failures)

    if failures:
        print("FAILED:")
        for item in failures:
            print(" -", item)
        return 1

    print(f"OK: {10} motorcycle drive-model smoke checks passed")
    return 0


if __name__ == "__main__":
    sys.exit(main())
